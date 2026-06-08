using EnvironmentMonitor.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface ILocationCommandService
    {
        Task SetMotionControlStatus(Guid locationIdentifier, MotionControlStatus status, DateTime? triggeringTime = null);
        Task SetMotionControlDelay(Guid locationIdentifier, long delayMs, DateTime? triggeringTime = null);
    }
}
