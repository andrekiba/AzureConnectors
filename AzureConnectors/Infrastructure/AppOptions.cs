namespace AzureConnectors.Infrastructure
{
    public class AppOptions
    {
        public string TeamsConnection { get; set; }
        public string AzureVMConnection { get; set; }
        public string TextAnalyticsConnection { get; set; }
        public string OutlookConnection { get; set; }
        public string AzureDevOpsConnection { get; set; }
        public string LinkedInConnection { get; set; }
    }
}