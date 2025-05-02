using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class FileUploadSettings
    {
        public uint MaxImageUploadSizeMb { get; set; } = 6;
        public uint MaxImageSaveSizeMb { get; set; } = 3;
        public uint ImageSaveQuality { get; set; } = 100;

        public uint ImageMinWidht { get; set; } = 300;
        public uint ImageMinHeight { get; set; } = 300;
    }
}
