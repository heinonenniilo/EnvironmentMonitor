using System.Collections.Generic;

namespace EnvironmentMonitor.Application.DTOs
{
    /// <summary>
    /// Request model for syncing a batch of measurements to another EnvironmentMonitor instance
    /// </summary>
    public class SyncMeasurementsRequest
    {
        /// <summary>
        /// Batch of measurements to sync
        /// </summary>
        public List<SaveMeasurementsDto> Measurements { get; set; } = new();
    }
}
