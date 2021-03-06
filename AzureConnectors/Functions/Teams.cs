using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Connectors.MicrosoftTeams;
using Azure.Connectors.MicrosoftTeams.Models;
using Azure.Connectors.TextAnalytics;
using Azure.Connectors.TextAnalytics.Models;
using AzureConnectors.Infrastructure;
using AzureConnectors.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.Azure;
using Microsoft.Rest.TransientFaultHandling;
using Newtonsoft.Json;

namespace AzureConnectors.Functions
{
    public class Teams
    {
        readonly AppOptions options;

        public Teams(IOptions<AppOptions> options)
        {
            this.options = options.Value;
        }
        
        [FunctionName("Spammer")]
        public async Task<IActionResult> Spammer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "teams/spammer")] HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var teamsInfo = JsonConvert.DeserializeObject<TeamsInfo>(requestBody);
                
                var teamsConnector = MicrosoftTeamsConnector.Create(options.TeamsConnection);
                
                var teams = await teamsConnector.GetAllTeamsAsync();
                var team = teams.Value.Single(t => t.DisplayName == teamsInfo.Team);
                
                var channels = await teamsConnector.GetChannelsForGroupAsync(team.Id);
                var channel = channels.Value.Single(c => c.DisplayName == teamsInfo.Channel);
                
                await teamsConnector.PostMessageToChannelV3Async(team.Id, channel.Id, new PostMessageToChannelV3Body
                {
                    Subject = "Spam for KLab from #ScottishSummit2021 :-)",
                    Body = new PostMessageToChannelV3BodyBody
                    {
                        Content = $"Good morning {teamsInfo.Team}!",
                        ContentType = "text"
                    }
                });

                //await teamsConnector.PostChannelAdaptiveCardAsync(team.Id, null);
                //await teamsConnector.PostUserAdaptiveCardAsync(null);
                
                return new OkResult();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
        
        [FunctionName("Sentiment")]
        public async Task<IActionResult> Sentiment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "teams/sentiment")] HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var teamsInfo = JsonConvert.DeserializeObject<TeamsInfo>(requestBody);
                
                var teamsConnector = MicrosoftTeamsConnector.Create(options.TeamsConnection);
                
                var retryStrategy = new FixedIntervalRetryStrategy(2, TimeSpan.FromSeconds(2));
                var retryPolicy = new RetryPolicy(new HttpStatusCodeErrorDetectionStrategy(), retryStrategy);
                teamsConnector.SetRetryPolicy(retryPolicy);
                
                var teams = await teamsConnector.GetAllTeamsAsync();
                var klab = teams.Value.Single(t => t.DisplayName == teamsInfo.Team);
                var channels = await teamsConnector.GetChannelsForGroupAsync(klab.Id);
                var general = channels.Value.Single(c => c.DisplayName == teamsInfo.Channel);
                
                string lastMessage;
                try
                {
                    var messages = await teamsConnector.GetMessagesFromChannelAsync(klab.Id, general.Id);
                    //while (messages.NextPageLink != null)
                    //    messages = await teamsConnector.GetMessagesFromChannelNextAsync(messages.NextPageLink);
                    lastMessage = messages.First().Body.Content;//.StripHtml();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(lastMessage);
                    lastMessage = doc.DocumentNode.InnerText;
                }
                catch (CloudException cloudException) 
                {
                    // provide below information when reporting runtime issues 
                    // "x-ms-client-request-id", RequestUri and Method
                    var requestUri = cloudException.Request.RequestUri.ToString();
                    var clientRequestId = cloudException.Request.Headers["x-ms-client-request-id"].First();
                    var statusCode = cloudException.Response.StatusCode.ToString();
                    var reasonPhrase = cloudException.Response.ReasonPhrase;
                    log.LogError($"RequestUri '{requestUri}' ClientRequestId '{clientRequestId}' StatusCode '{statusCode}' ReasonPhrase '{reasonPhrase}'");
                    throw;
                }
                
                var cognitiveTextAnalyticsService = TextAnalyticsConnector.Create(options.TextAnalyticsConnection);
                cognitiveTextAnalyticsService.SetRetryPolicy(new RetryPolicy<HttpStatusCodeErrorDetectionStrategy>(3));
                var sentimentScore = await cognitiveTextAnalyticsService.Sentiment.DetectSentimentV2Async(new MultiLanguageInput { Language = "it", Text = lastMessage });
            
                return new OkObjectResult(sentimentScore);
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
    }
}