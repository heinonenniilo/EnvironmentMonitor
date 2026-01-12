namespace EnvironmentMonitor.Domain.Models
{
    public class GitHubSettings
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string? GitHubHost { get; set; }
    }
}
