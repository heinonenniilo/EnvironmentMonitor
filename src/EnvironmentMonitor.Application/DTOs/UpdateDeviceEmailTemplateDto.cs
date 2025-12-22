using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Application.DTOs
{
    public class UpdateDeviceEmailTemplateDto
    {
        public Guid Identifier { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
    }
}
