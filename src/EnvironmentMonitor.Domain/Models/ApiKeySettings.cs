namespace EnvironmentMonitor.Domain.Models
{
    public class ApiKeySettings
    {
        public List<ApiKeyConfig> ApiKeys { get; set; } = new List<ApiKeyConfig>();
    }

    public class ApiKeyConfig
    {
        public string Key { get; set; } = string.Empty;
        public string Devices { get; set; } = string.Empty;
    }
}
