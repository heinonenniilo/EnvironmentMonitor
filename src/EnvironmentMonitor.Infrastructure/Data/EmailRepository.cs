using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class EmailRepository : IEmailRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly IDateService _dateService;

        public EmailRepository(MeasurementDbContext context, IDateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }

        public async Task<EmailTemplate?> GetEmailTemplate(DeviceEmailTemplateTypes templateType)
        {
            return await _context.EmailTemplates
                .FirstOrDefaultAsync(x => x.Id == (int)templateType);
        }

        public async Task<EmailTemplate?> GetEmailTemplateByIdentifier(Guid identifier)
        {
            return await _context.EmailTemplates
                .FirstOrDefaultAsync(x => x.Identifier == identifier);
        }

        public async Task<List<EmailTemplate>> GetAllEmailTemplates()
        {
            return await _context.EmailTemplates
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<EmailTemplate> UpdateEmailTemplate(Guid identifier, string? title, string? message, bool saveChanges)
        {
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(x => x.Identifier == identifier);

            if (template == null)
            {
                throw new EntityNotFoundException($"Email template with identifier: {identifier} not found.");
            }

            template.Title = title;
            template.Message = message;

            var now = _dateService.CurrentTime();
            template.Updated = now;

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            return template;
        }
    }
}
