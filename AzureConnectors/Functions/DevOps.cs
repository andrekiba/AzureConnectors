using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Connectors.AzureDevOps;
using Azure.Connectors.AzureDevOps.Models;
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
    public class DevOps
    {
        readonly AppOptions options;

        public DevOps(IOptions<AppOptions> options)
        {
            this.options = options.Value;
        }
        
        [FunctionName("CreateRelease")]
        public async Task<IActionResult> CreateRelease(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "devops/createrelease")] HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var newRelease = JsonConvert.DeserializeObject<CreateRelease>(requestBody);
                
                var devOpsConnector = AzureDevOpsConnector.Create(options.AzureDevOpsConnection);
                
                var accounts = await devOpsConnector.VisualStudioTeamServices.ListAccountsAsync();
                var account = accounts.Value.Single(a => a.AccountName == newRelease.Account);
                
                var projects = await devOpsConnector.VisualStudioTeamServices.ListProjectsAsync(account.AccountName);
                var project = projects.Value.Single(p => p.Name == newRelease.Project);
                
                var definitions = await devOpsConnector.VisualStudioTeamServices.ListReleaseDefinitionsAsync(account.AccountName, project.Name);
                var release = definitions.Value.Single(rd => rd.Name == newRelease.ReleaseDefinition);
                
                await devOpsConnector.VisualStudioTeamServices.CreateReleaseAsync(account.AccountName, project.Name,
                    release.Id.ToString(), new ReleaseStartMetadata("Test from Azure DevOps Connector", true, "Test"));
                
                return new OkResult();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
    }
}