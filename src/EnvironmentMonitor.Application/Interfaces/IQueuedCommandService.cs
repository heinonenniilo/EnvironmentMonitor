using EnvironmentMonitor.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IQueuedCommandService
    {
        Task<List<DeviceQueuedCommandDto>> GetQueuedCommands(Guid deviceIdentifier);
        Task<DeviceQueuedCommandDto> UpdateQueuedCommandSchedule(UpdateQueuedCommand model);
        Task RemoveQueuedCommand(Guid deviceIdentifier, string messageId);
        Task<DeviceQueuedCommandDto> CopyExecutedQueuedCommand(CopyQueuedCommand model);
        /// <summary>
        /// Null date indicates error
        /// </summary>
        /// <param name="identifier">Device identifier</param>
        /// <param name="messageId">message id</param>
        /// <param name="date">When completed. If NULL, will be interpreted as failure to complete.</param>
        /// <returns></returns>
        Task AckQueuedCommand(Guid identifier, string messageId, DateTime? date);
    }
}
