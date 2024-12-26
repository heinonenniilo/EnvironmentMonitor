using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class DeviceInfo
    {
        public Device Device { get; set; }
        public DateTime? OnlineSince { get; set; }
        public DateTime? RebootedOn { get; set; }

        public DateTime? LastMessage { get; set; }
    }
}
