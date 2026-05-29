using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class AddDeviceDto
    {
        public required string Name { get; set; }
        public required string DeviceIdentifier { get; set; }
        public bool Visible { get; set; }
        public int? CommunicationChannelId { get; set; }
    }
}
