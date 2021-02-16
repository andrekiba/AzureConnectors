using System.Text.Json.Serialization;

namespace AzureConnectors.Models
{
    public class AzureVM
    {
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Name { get; set; }
    }
}