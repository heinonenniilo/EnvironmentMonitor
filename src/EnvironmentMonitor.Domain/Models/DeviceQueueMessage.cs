using System;

namespace EnvironmentMonitor.Domain.Models
{
    public class DeviceQueueMessage
    {
        public Guid DeviceIdentifier { get; set; }
        public int MessageTypeId { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
    }
}
