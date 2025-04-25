using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceAttachment
    {
        public Guid Guid { get; set; }
        public Device Device { get; set; }
        public int DeviceId { get; set; }
        public int AttachmentId { get; set; }
        public Attachment Attachment { get; set; }
        public bool IsImage { get; set; } = true;
        public bool IsDefaultImage { get; set; }
    }
}
