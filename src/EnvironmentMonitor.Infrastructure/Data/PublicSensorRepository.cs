using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class PublicSensorRepository : IPublicSensorRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly IDateService _dateService;
        private readonly ILogger<PublicSensorRepository> _logger;

        public PublicSensorRepository(MeasurementDbContext context, IDateService dateService, ILogger<PublicSensorRepository> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
        }

        public async Task<List<PublicSensor>> GetPublicSensors(List<Guid>? identifiers = null)
        {
            IQueryable<PublicSensor> query = _context.PublicSensors;

            if (identifiers != null && identifiers.Any())
            {
                query = query.Where(ps => identifiers.Contains(ps.Identifier));
            }

            return await query
                .Include(ps => ps.Sensor)
                .Include(ps => ps.MeasurementType)
                .ToListAsync();
        }

        public async Task<PublicSensor?> GetPublicSensor(Guid identifier)
        {
            return await _context.PublicSensors
                .Include(ps => ps.Sensor)
                .Include(ps => ps.MeasurementType)
                .FirstOrDefaultAsync(ps => ps.Identifier == identifier);
        }

        public async Task<PublicSensor> AddPublicSensor(PublicSensor publicSensor, bool saveChanges)
        {
            await _context.PublicSensors.AddAsync(publicSensor);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return publicSensor;
        }

        public async Task<PublicSensor> UpdatePublicSensor(PublicSensor publicSensor, bool saveChanges)
        {
            var now = _dateService.CurrentTime();
            publicSensor.Updated = now;
            publicSensor.UpdatedUtc = _dateService.LocalToUtc(now);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return publicSensor;
        }

        public async Task DeletePublicSensor(PublicSensor publicSensor, bool saveChanges)
        {
            _context.PublicSensors.Remove(publicSensor);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
