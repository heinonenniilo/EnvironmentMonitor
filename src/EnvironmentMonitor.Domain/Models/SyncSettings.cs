namespace EnvironmentMonitor.Domain.Models
{
    /// <summary>
    /// Configuration settings for syncing measurements to another EnvironmentMonitor instance
    /// </summary>
    public class SyncSettings
    {
        /// <summary>
        /// Target URL for the sync endpoint (e.g., https://target-instance.com/api/measurements/sync)
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// API Key for authentication with the target instance (X-API-KEY header)
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Secret ID for authentication with the target instance (X-SECRET-ID header)
        /// </summary>
        public string SecretId { get; set; } = string.Empty;

        /// <summary>
        /// Secret value for authentication with the target instance (X-SECRET-VALUE header)
        /// </summary>
        public string SecretValue { get; set; } = string.Empty;

        /// <summary>
        /// Whether sync is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Maximum number of messages to sync in one batch (default: 40)
        /// </summary>
        public int BatchSize { get; set; } = 40;

        /// <summary>
        /// CommunicationChannel IDs to exclude from sync. If empty or null, all channels are included.
        /// </summary>
        public List<int> ExcludedCommunicationChannels { get; set; } = new List<int>();

        /// <summary>
        /// Whether to skip status check when processing incoming sync measurements
        /// </summary>
        public bool SkipStatusCheck { get; set; } = true;

        public bool HandleFirstMessages { get; set; } = false;
    }
}
