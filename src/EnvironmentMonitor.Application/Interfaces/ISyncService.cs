using EnvironmentMonitor.Application.DTOs;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    /// <summary>
    /// Service for syncing device messages to another EnvironmentMonitor instance
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Syncs unsynced device messages to the configured target instance.
        /// Processes up to BatchSize messages at a time.
        /// </summary>
        /// <returns>Number of messages synced</returns>
        Task<int> SyncMessages();

        /// <summary>
        /// Processes incoming sync measurements from another EnvironmentMonitor instance.
        /// Validates secret and imports measurements with status checks disabled.
        /// </summary>
        /// <param name="request">The sync request containing measurements and secret</param>
        /// <returns>Sync result with counts</returns>
        Task<SyncResultDto> ProcessIncomingSync(SyncMeasurementsRequest request);
    }
}
