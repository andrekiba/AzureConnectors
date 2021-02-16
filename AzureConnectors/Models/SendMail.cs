namespace AzureConnectors.Models
{
    public class SendMail
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string To { get; set; }
    }
}