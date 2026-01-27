using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class Device
    {
        public int Id { get; set; }
        public Guid Identifier { get; set; }
        public required string DeviceIdentifier { get; set; }
        public string Name { get; set; }
        public ICollection<Sensor> Sensors { get; set; }
        public ICollection<DeviceEvent> Events { get; set; }
        public bool Visible { get; set; }
        public int? TypeId { get; set; }
        public DeviceType? Type { get; set; }
        public bool HasMotionSensor { get; set; }
        public Location Location { get; set; }
        public int LocationId { get; set; }
        public IList<LocationSensor> LocationSensors { get; set; } = [];
        public IList<DeviceAttachment> Attachments { get; set; } = [];
        public IList<DeviceStatus> StatusChanges { get; set; } = [];
        public IList<DeviceMessage> DeviceMessages { get; set; } = [];
        public IList<DeviceAttribute> DeviceAttributes { get; set; } = [];
        public IList<DeviceQueuedCommand> QueuedCommands { get; set; } = [];
        public IList<DeviceContact> Contacts { get; set; } = [];
        public bool IsVirtual { get; set; }
        public int? CommunicationChannelId { get; set; }
        public CommunicationChannel? CommunicationChannel { get; set; }
    }
}
