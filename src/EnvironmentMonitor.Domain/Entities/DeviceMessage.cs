using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceMessage : TrackedEntity
    {
        public long Id { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public required DateTime TimeStamp { get; set; }
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;
        public IList<Measurement> Measurements { get; set; } = [];
        public IList<DeviceStatus> DeviceStatuses { get; set; } = [];
        public long? SequenceNumber { get; set; }
        public bool FirstMessage { get; set; }
        public long? Uptime { get; set; }
        public string? Identifier { get; set; }
        public long? LoopCount { get; set; }
        public long? MessageCount { get; set; }
        public bool IsDuplicate { get; set; }
    }
}
