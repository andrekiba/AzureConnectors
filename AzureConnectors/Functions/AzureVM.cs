using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Connectors.AzureVM;
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
                var sub = subs.Value.Single(x => x.SubscriptionId == vmInfo.SubscriptionId);
                
                var rgs = await azureVMConnector.ResourceGroups.ListAsync(ApiVersion, sub.SubscriptionId);
                var rg = rgs.Value.Single(x => x.Name == vmInfo.ResourceGroup);
                
                var vms = await azureVMConnector.VirtualMachines.ListAsync(ApiVersion, sub.SubscriptionId, rg.Name);
                var vm = vms.Value.Single(x => x.Name == vmInfo.Name);
                
                await azureVMConnector.VirtualMachines.VirtualMachinePoweroffAsync(sub.SubscriptionId, rg.Name, vm.Name);
                
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
                
                //var subs = await azureVMConnector.Subscriptions.ListAsync(ApiVersion);
                //var sub = subs.Value.Single(m => m.SubscriptionId == vmInfo.SubscriptionId);
                
                await azureVMConnector.VirtualMachines.VirtualMachinePoweroffAsync(vmInfo.SubscriptionId, vmInfo.ResourceGroup, vmInfo.Name);
                
                return new OkResult();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }
    }
}