using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceMessage
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public required DateTime TimeStamp { get; set; }
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;
        public IList<Measurement> Measurements { get; set; } = [];
        public IList<DeviceStatus> DeviceStatuses { get; set; } = [];
        public long? SequenceNumber { get; set; }
    }
}
