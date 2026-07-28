using EnvironmentMonitor.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    /// <summary>
    /// Repository for managing application-level status variables
    /// </summary>
    public interface IStatusVariableRepository
    {
        /// <summary>
        /// Gets a status variable by its key
        /// </summary>
        Task<StatusVariable?> GetByKey(string key);

        /// <summary>
        /// Sets or updates a status variable value
        /// </summary>
        Task SetValue(string key, string value);

        /// <summary>
        /// Gets unsynced device messages with related data for sync operation
        /// </summary>
        Task<List<DeviceMessage>> GetUnsyncedDeviceMessages(long lastSyncedId, int batchSize);
    }
}
