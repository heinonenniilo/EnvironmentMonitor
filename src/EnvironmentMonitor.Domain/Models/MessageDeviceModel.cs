using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class MessageDeviceModel
    {
        public required List<Guid> DeviceIdentifiers { get; set; }
        public DateTime? ExecuteAt { get; set; }
    }

    public class SetMotionControlStatusMessage : MessageDeviceModel
    {
        public int Mode { get; set; }
    }

    public class SetMotionControlDelayMessage : MessageDeviceModel
    {
        public long DelayMs { get; set; }
    }

    public class SetDefaultImage : MessageDeviceModel
    {
        public Guid AttachmentGuid { get; set; }
    }
}
