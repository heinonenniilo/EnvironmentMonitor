using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
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
        private readonly IDateService _dateService;
        private readonly ILogger<MeasurementRepository> _logger;
        public MeasurementRepository(MeasurementDbContext context, IDateService dateService, ILogger<MeasurementRepository> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
        }

        public async Task<IEnumerable<Measurement>> GetMeasurements(GetMeasurementsModel model)
        {
            var query = _context.Measurements
                .Where(x =>
                model.SensorIds.Contains(x.SensorId));
            if (model.LatestOnly == true)
            if (model.LatestOnly == true)
            {
                var grouped = await query.Where(
                    x => x.Timestamp > _dateService.CurrentTime().AddDays(-1 * ApplicationConstants.DeviceLastMessageFetchLimitIndays))
                    .GroupBy(x => new { x.SensorId, x.TypeId }).Select(d => new
                    {
                        Id = d.Max(x => x.Id),
                    }).ToListAsync();
                var latestMeasurements = _context.Measurements.Where(x => grouped.Select(g => g.Id).Contains(x.Id)).OrderByDescending(x => x.Timestamp);
                return await latestMeasurements.ToListAsync();
            }
            else
            {
                var dateDiff = ((model.To ?? _dateService.CurrentTime()) - model.From).TotalDays;
                query = query.Where(x => x.Timestamp >= model.From && (model.To == null || x.Timestamp <= model.To));
                if (dateDiff > ApplicationConstants.MeasurementGroupByLimitInDays)
                {
                    _logger.LogInformation($"Applying measurement grouping, date diff: {dateDiff}, limit : {ApplicationConstants.MeasurementGroupByLimitInDays}");
                    query = query.GroupBy(x => new
                    {
                        x.TypeId,
                        x.SensorId,
                        x.Timestamp.Year,
                        x.Timestamp.Month,
                        x.Timestamp.Day,
                        x.Timestamp.Hour
                    }).Select(x => new Measurement()
                    {
                        TypeId = x.Key.TypeId,
                        TimestampUtc = DateTime.UtcNow,
                        Timestamp = x.Max(d => d.Timestamp),
                        Value = x.Key.TypeId == (int)MeasurementTypes.Motion ? x.Max(d => d.Value) : x.Average(d => d.Value),
                        SensorId = x.Key.SensorId
                    }).OrderBy(x => x.Timestamp);
                    var rows = await query.ToListAsync();
                    foreach (var item in rows)
                    {
                        item.TimestampUtc = _dateService.LocalToUtc(item.Timestamp);
                    }
                    return rows;
                }
                query = query.OrderBy(x => x.Timestamp);
            }
            return await query.ToListAsync();
        }

        public async Task<IList<Measurement>> AddMeasurements(List<Measurement> measurements, bool saveChanges = true, DeviceMessage? deviceMessage = null)
        {
            await _context.Measurements.AddRangeAsync(measurements.Select(m =>
            {
                m.DeviceMessage = deviceMessage;
                return m;
            }));
            if (saveChanges)
            {
                _logger.LogInformation("Saving changes (AddMeasurements)");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save");
                }
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
