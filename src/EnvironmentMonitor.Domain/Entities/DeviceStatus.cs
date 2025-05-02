using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceStatus
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public required bool Status { get; set; }
        public required DateTime TimeStamp { get; set; }
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;
        public string? Message { get; set; }
    }
}
