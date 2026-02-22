namespace EnvironmentMonitor.Domain.Models
{
    public class ApiKeySettings
    {
        public List<string> ApiKeys { get; set; } = [];
        public int ApiKeyCacheExpirationMinutes { get; set; } = 30;
    }
}
