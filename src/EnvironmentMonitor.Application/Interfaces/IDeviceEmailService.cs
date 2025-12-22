using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceEmailService
    {
        Task<DeviceEmailTemplateDto?> GetEmailTemplate(DeviceEmailTemplateTypes templateType);
        Task<DeviceEmailTemplateDto?> GetEmailTemplateByIdentifier(Guid identifier);
        Task<List<DeviceEmailTemplateDto>> GetAllEmailTemplates();
        Task<DeviceEmailTemplateDto> UpdateEmailTemplate(UpdateDeviceEmailTemplateDto model);
        Task SendDeviceEmail(Guid deviceIdentifier, DeviceEmailTemplateTypes templateType, Dictionary<string, string>? replaceTokens = null);
    }
}
