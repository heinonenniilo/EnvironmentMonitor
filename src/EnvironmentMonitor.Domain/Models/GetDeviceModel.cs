using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class GetDeviceModel
    {
        public List<int>? Ids { get; set; }
        public List<string>? DeviceIdentifiers { get; set; }
        public List<Guid>? Identifiers { get; set; }
        public List<int>? LocationIds { get; set; }
        public bool OnlyVisible { get; set; }
        public bool GetAttachments { get; set; }
    }
}
