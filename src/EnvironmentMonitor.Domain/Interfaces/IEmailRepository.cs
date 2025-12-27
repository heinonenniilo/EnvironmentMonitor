using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IEmailRepository
    {
        Task<EmailTemplate?> GetEmailTemplate(DeviceEmailTemplateTypes templateType);
        Task<EmailTemplate?> GetEmailTemplateByIdentifier(Guid identifier);
        Task<List<EmailTemplate>> GetAllEmailTemplates();
        Task<EmailTemplate> UpdateEmailTemplate(Guid identifier, string? title, string? message, bool saveChanges);
    }
}
