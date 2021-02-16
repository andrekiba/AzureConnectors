using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Connectors.AzureVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Connectors.MicrosoftTeams;
using AzureConnectors.Infrastructure;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureConnectors.Models;
using Newtonsoft.Json;

namespace AzureConnectors
{
    public class AzureVM
    {
        readonly AppOptions options;
        const string ApiVersion = "2020-06-01";
        
        public AzureVM(IOptions<AppOptions> options)
        {
            this.options = options.Value;
        }
        
        [FunctionName("Poweroff")]
        public async Task<IActionResult> Poweroff(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "vm/poweroff")] HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var vmInfo = JsonConvert.DeserializeObject<AzureVMInfo>(requestBody);
                var azureVMConnector = AzureVMConnector.Create(options.AzureVMConnection);
                var subs = await azureVMConnector.Subscriptions.ListAsync(ApiVersion);
                var ambrogioSaaS = subs.Value.Single(m => m.DisplayName == vmInfo.Subscription);
                var rgs = await azureVMConnector.ResourceGroups.ListAsync(ApiVersion, ambrogioSaaS.SubscriptionId);
                var rg = rgs.Value.Single(x => x.Name == "AmbrogioSaas-Dev");
                var vms = await azureVMConnector.VirtualMachines.ListAsync(ApiVersion, ambrogioSaaS.SubscriptionId, rg.Name);
                var ambrogioVm = vms.Value.Single(vm => vm.Name == "ambrogio-dev");
                await azureVMConnector.VirtualMachines.VirtualMachinePoweroffAsync(ambrogioSaaS.SubscriptionId, rg.Name, ambrogioVm.Name);
                
                return new OkResult();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
        
        [FunctionName("Poweron")]
        public async Task<IActionResult> Poweron(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "vm/poweron")] HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var vmInfo = JsonConvert.DeserializeObject<AzureVMInfo>(requestBody);
                var azureVMConnector = AzureVMConnector.Create(options.AzureVMConnection);
                var subs = await azureVMConnector.Subscriptions.ListAsync(ApiVersion);
                var ambrogioSaaS = subs.Value.Single(m => m.DisplayName == vmInfo.Subscription);
                await azureVMConnector.VirtualMachines.VirtualMachinePoweroffAsync(ambrogioSaaS.SubscriptionId, vmInfo.ResourceGroup, vmInfo.Name);
                
                return new OkResult();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
    }
}