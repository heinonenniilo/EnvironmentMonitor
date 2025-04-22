using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class UploadAttachmentModel
    {
        public required Stream Stream { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
    }
}
