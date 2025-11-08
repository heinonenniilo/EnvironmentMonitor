using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceAttribute : TrackedEntity
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public int TypeId { get; set; }
        public DeviceAttributeType Type { get; set; }
        public string? Value { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime TimeStampUtc { get; set; }
    }
}
