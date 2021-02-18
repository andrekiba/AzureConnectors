using System;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Connectors.LinkedInV2;
using Azure.Connectors.LinkedInV2.Models;
using AzureConnectors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureConnectors.Functions
{
    public class LinkedIn
    {
        readonly AppOptions options;

        public LinkedIn(IOptions<AppOptions> options)
        {
            this.options = options.Value;
        }
    
        [FunctionName("LinkedInTest")]
        public async Task<IActionResult> LinkedInTest(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "linkedin/test")] HttpRequest req, ILogger log)
        {
            try
            {
                var linkedinConnector = LinkedInV2Connector.Create(options.LinkedInConnection);
                var post = new ShareArticleRequestV2
                {
                    Text = new ShareArticleRequestV2Text("Azure Connectors for #AzureFunctions - Live from #GlobalPowerPlatformBootcamp"),
                    Content = new ShareArticleRequestV2Content
                    {
                        ContentUrl = "https://www.powerplatformbootcamp.com/2021/location-detail/?id=d443a6cb-a912-eb11-8441-0003ffb4cdf8&city=Milano"
                    },
                    Distribution = new ShareArticleRequestV2Distribution
                    {
                        LinkedInDistributionTarget = new ShareArticleRequestV2DistributionLinkedInDistributionTarget
                        {
                            VisibleToGuest = true
                        }
                    }
                };
                await linkedinConnector.ShareUpdateV2Async(post);
                
                return new OkResult();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
    }
}