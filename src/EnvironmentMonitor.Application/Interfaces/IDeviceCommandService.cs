using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceCommandService
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
        
        // Device command methods moved from IDeviceService
        Task Reboot(Guid identifier);
        Task<List<DeviceAttributeDto>> SetMotionControlStatus(Guid identifier, MotionControlStatus status, DateTime? triggeringTime = null);
        Task<List<DeviceAttributeDto>> SetMotionControlDelay(Guid identifier, long delayMs, DateTime? triggeringTime = null);
        Task SendAttributesToDevice(Guid identifier, string? message = null);
    }
}
