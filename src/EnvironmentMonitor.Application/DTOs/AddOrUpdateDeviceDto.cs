using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class AddOrUpdateDeviceDto
    {
        /// <summary>
        /// When set, the existing device with this identifier will be updated. When null, a new device is created.
        /// </summary>
        public Guid? Identifier { get; set; }
        public required string Name { get; set; }
        public required string DeviceIdentifier { get; set; }
        public bool Visible { get; set; }
        public int? CommunicationChannelId { get; set; }
    }
}
