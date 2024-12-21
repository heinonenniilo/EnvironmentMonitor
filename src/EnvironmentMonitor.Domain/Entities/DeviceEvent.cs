using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceEvent
    {
        public DateTime TimeStamp { get; set; }
        public DateTime TimeStampUtc { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public string? Message { get; set; }
        public long Id { get; set; }
        public int TypeId { get; set; }
        public DeviceEventType Type { get; set; }
    }
}
