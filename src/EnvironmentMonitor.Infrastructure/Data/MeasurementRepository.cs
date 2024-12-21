using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class MeasurementRepository : IMeasurementRepository
    {
        private readonly MeasurementDbContext _context;
        public MeasurementRepository(MeasurementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Measurement>> GetMeasurements(GetMeasurementsModel model)
        {
            var query = _context.Measurements
                .Where(x =>
                model.SensorIds.Contains(x.SensorId));
            if (model.LatestOnly == true)
            {
                var grouped = await query.GroupBy(x => new { x.SensorId, x.TypeId }).Select(d => new
                {
                    Id = d.Max(x => x.Id),
                }).ToListAsync();
                var latestMeasurements = _context.Measurements.Where(x => grouped.Select(g => g.Id).Contains(x.Id)).OrderByDescending(x => x.Timestamp);
                return await latestMeasurements.ToListAsync();
            }
            else
            {
                query = query
                    .Where(x => x.Timestamp >= model.From && (model.To == null || x.Timestamp <= model.To))
                    .OrderBy(x => x.Timestamp);
            }
            return await query.ToListAsync();
        }

        public async Task<IList<Measurement>> AddMeasurements(List<Measurement> measurements, bool saveChanges = true)
        {
            await _context.Measurements.AddRangeAsync(measurements);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return measurements;
        }

        public async Task<MeasurementType?> GetMeasurementType(int id)
        {
            var type = await _context.MeasurementTypes.FirstOrDefaultAsync(x => x.Id == id);
            return type;
        }

        public async Task<IEnumerable<Measurement>> Get(
            Expression<Func<Measurement, bool>> filter = null,
            Func<IQueryable<Measurement>, IOrderedQueryable<Measurement>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<Measurement> query = _context.Measurements;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }
    }
}
