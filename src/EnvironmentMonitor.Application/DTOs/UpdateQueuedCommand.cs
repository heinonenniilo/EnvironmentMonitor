using System;

namespace EnvironmentMonitor.Application.DTOs
{
    public class UpdateQueuedCommand
    {
        public required Guid DeviceIdentifier { get; set; }
        public required string MessageId { get; set; }
        public required DateTime NewScheduledTime { get; set; }
    }
}
