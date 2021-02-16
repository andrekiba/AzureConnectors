using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Connectors.Office365Outlook;
using Azure.Connectors.Office365Outlook.Models;
using AzureConnectors.Infrastructure;
using AzureConnectors.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AzureConnectors.Functions
{
    public class Outlook
    {
        readonly AppOptions options;

        public Outlook(IOptions<AppOptions> options)
        {
            this.options = options.Value;
        }
        
        [FunctionName("SendMail")]
        public async Task<IActionResult> SendMail(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "outlook/sendmail")] HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var mail = JsonConvert.DeserializeObject<SendMail>(requestBody);
                var outlookConnector = Office365OutlookConnector.Create(options.OutlookConnection);
                await outlookConnector.Mail.SendEmailV2Async(new ClientSendHtmlMessage
                {
                    Subject = mail.Subject,
                    Body = mail.Body,
                    To = mail.To
                });

                return new OkResult();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
    }
}