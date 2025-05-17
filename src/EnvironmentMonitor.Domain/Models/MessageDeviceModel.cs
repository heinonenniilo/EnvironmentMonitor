using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class MessageDeviceModel
    {
        public required Guid DeviceIdentifier { get; set; }
    }

    public class SetMotionControlStatusMessage : MessageDeviceModel
    {
        public int Mode { get; set; }
    }

    public class SetMotionControlDelayMessag : MessageDeviceModel
    {
        public long DelayMs { get; set; }
    }

    public class SetDefaultImage: MessageDeviceModel
    {
        public Guid AttachmentGuid { get; set; }
    }
}
