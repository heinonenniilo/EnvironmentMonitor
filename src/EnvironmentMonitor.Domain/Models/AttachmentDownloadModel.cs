using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{

    public class AttachmentDownloadModel : AttachmentInfoModel
    {
        public required Stream Stream { get; set; }
    }

    public class AttachmentInfoModel
    {
        public required string ContentType { get; set; }
        public long SizeInBytes { get; set; }
    }
}
