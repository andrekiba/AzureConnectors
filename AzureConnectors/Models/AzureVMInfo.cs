using System.Text.Json.Serialization;

namespace AzureConnectors.Models
{
    public class AzureVMInfo
    {
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string Name { get; set; }
    }
}