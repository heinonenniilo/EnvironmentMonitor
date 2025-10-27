using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.AddModels
{
    public class UploadAttachmentModel
    {
        public required Stream Stream { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public bool IsSecret { get; set; }
    }

    public class UploadDeviceAttachmentModel : UploadAttachmentModel
    {
        public required Guid DeviceIdentifier { get; set; }
        public required bool IsDeviceImage { get; set; }
    }
}
