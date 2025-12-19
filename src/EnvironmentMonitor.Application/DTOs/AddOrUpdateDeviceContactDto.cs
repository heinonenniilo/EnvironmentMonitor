using System;

namespace EnvironmentMonitor.Application.DTOs
{
    public class AddOrUpdateDeviceContactDto
    {
        public Guid? Identifier { get; set; }
        public Guid DeviceIdentifier { get; set; }
        public required string Email { get; set; }
    }
}
