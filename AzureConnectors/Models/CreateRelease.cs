namespace AzureConnectors.Models
{
    public class CreateRelease
    {
        public string Account { get; set; }
        public string Project { get; set; }
        public string ReleaseDefinition { get; set; }
    }
}