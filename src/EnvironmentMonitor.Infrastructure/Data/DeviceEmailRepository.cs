using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class DeviceEmailRepository : IDeviceEmailRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly IDateService _dateService;

        public DeviceEmailRepository(MeasurementDbContext context, IDateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }

        public async Task<DeviceEmailTemplate?> GetEmailTemplate(DeviceEmailTemplateTypes templateType)
        {
            return await _context.DeviceEmailTemplates
                .FirstOrDefaultAsync(x => x.Id == (int)templateType);
        }

        public async Task<DeviceEmailTemplate?> GetEmailTemplateByIdentifier(Guid identifier)
        {
            return await _context.DeviceEmailTemplates
                .FirstOrDefaultAsync(x => x.Identifier == identifier);
        }

        public async Task<List<DeviceEmailTemplate>> GetAllEmailTemplates()
        {
            return await _context.DeviceEmailTemplates
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<DeviceEmailTemplate> UpdateEmailTemplate(Guid identifier, string? title, string? message, bool saveChanges)
        {
            var template = await _context.DeviceEmailTemplates
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
