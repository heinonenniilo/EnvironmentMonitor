using System;

namespace EnvironmentMonitor.Application.DTOs
{
    public class CopyQueuedCommand
    {
        public required Guid DeviceIdentifier { get; set; }
        public required string MessageId { get; set; }
        public DateTime? ScheduledTime { get; set; }
    }
}
