using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class SetDeviceStatusModel
    {
        public bool? Status { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string? Details { get; set; }
        public int DeviceId { get; set; }
        public Guid Idenfifier { get; set; }
        public string? Message { get; set; }
        public DeviceMessage? DeviceMessage { get; set; }
    }
}
