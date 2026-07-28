namespace EnvironmentMonitor.Application.DTOs
{
    /// <summary>
    /// Result model for sync operations
    /// </summary>
    public class SyncResultDto
    {
        /// <summary>
        /// Number of measurements successfully synced
        /// </summary>
        public int SyncedCount { get; set; }

        /// <summary>
        /// Total number of measurements received
        /// </summary>
        public int TotalReceived { get; set; }

        /// <summary>
        /// Whether the sync operation was successful
        /// </summary>
        public bool Success => SyncedCount == TotalReceived && TotalReceived > 0;
    }
}
