using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{


    public class GetDevicesModel
    {
        public List<int>? Ids { get; set; }
        public List<string>? DeviceIdentifiers { get; set; }
        public List<Guid>? Identifiers { get; set; }
        public List<Guid>? LocationIdentifiers { get; set; }
        public List<int>? CommunicationChannelIds { get; set; }
        public bool OnlyVisible { get; set; }
        public bool GetAttachments { get; set; }
        public bool GetLocation { get; set; }
        public bool GetAttributes { get; set; }
        public bool GetContacts { get; set; }
        public bool GetSensors { get; set; }
    }

    public class GetSensorsModel
    {
        public required GetDevicesModel DevicesModel { get; set; }
        public List<int>? Ids { get; set; }
        public List<Guid>? Identifiers { get; set; }
        public List<int>? SensorIds { get; set; }
        public bool IncludeVirtualSensors { get; set; } = false;
    }
}
