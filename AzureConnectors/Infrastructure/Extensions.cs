using System.Text.RegularExpressions;

namespace AzureConnectors.Infrastructure
{
    public static class Extensions
    {
        public static string StripHtml(this string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}