using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class UpdateDeviceDto
    {
        public required DeviceDto Device { get; set; }
        public int? CommunicationChannelId { get; set; }
    }
}
