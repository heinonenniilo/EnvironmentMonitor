using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using LinqKit;
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

        public async Task<List<MeasurementExtended>> GetMeasurements(GetMeasurementsModel model)
        {

            if (((model.To ?? _dateService.CurrentTime()) - model.From).TotalDays > ApplicationConstants.MeasurementMaxLimitInDays)
            {
                throw new InvalidOperationException("Too long time range");
            }

            IQueryable<Measurement> query = _context.Measurements;
            if (model.SensorIds.Any())
            {
                query = query.Where(x => model.SensorIds.Contains(x.SensorId));
            }
            else if (model.SensorsByTypeFilter != null)
            {
                _logger.LogInformation("Dynamically building condition for measurement query (sensors with types defined)");
                var predicate = PredicateBuilder.New<Measurement>(false);
                foreach (var filter in model.SensorsByTypeFilter)
                {
                    predicate = predicate.Or(x =>
                        x.SensorId == filter.Key &&
                        (filter.Value == null || filter.Value.Count == 0 || filter.Value.Contains((MeasurementTypes)x.TypeId))
                    );
                }
                query = query.Where(predicate);
            }


            if (model.DeviceMessageIds?.Any() == true)
            {
                query = query.Where(x => x.DeviceMessageId != null && model.DeviceMessageIds.Contains(x.DeviceMessageId.Value));
                return await query.OrderBy(x => x.Timestamp).Select(x => new MeasurementExtended()
                {
                    TypeId = x.TypeId,
                    Value = x.Value,
                    Timestamp = x.Timestamp,
                    TimestampUtc = x.TimestampUtc,
                    SensorId = x.SensorId,
                    DeviceMessageId = x.DeviceMessageId,
                    SensorGuid = x.Sensor.Guid,
                }).ToListAsync();
            }
            if (model.LatestOnly == true)
            {
                var grouped = await query.Where(
                    x => x.Timestamp > _dateService.CurrentTime().AddDays(-1 * ApplicationConstants.DeviceLastMessageFetchLimitIndays))
                    .GroupBy(x => new { x.SensorId, x.TypeId }).Select(d => new
                    {
                        Id = d.Max(x => x.Id),
                    }).ToListAsync();
                var latestMeasurements = _context.Measurements.
                    Where(x => grouped.Select(g => g.Id).Contains(x.Id))
                    .Select(x => new MeasurementExtended()
                    {
                        TypeId = x.TypeId,
                        Value = x.Value,
                        Timestamp = x.Timestamp,
                        TimestampUtc = x.TimestampUtc,
                        SensorId = x.SensorId,
                        SensorGuid = x.Sensor.Guid,
                    })
                    .OrderByDescending(x => x.Timestamp);
                return await latestMeasurements.ToListAsync();
            }
            else
            {
                var dateDiff = ((model.To ?? _dateService.CurrentTime()) - model.From).TotalDays;
                query = query.Where(x => x.Timestamp >= model.From && (model.To == null || x.Timestamp <= model.To));
                if (dateDiff > ApplicationConstants.MeasurementGroupByLimitInDays)
                {
                    _logger.LogInformation($"Applying measurement grouping, date diff: {dateDiff}, limit : {ApplicationConstants.MeasurementGroupByLimitInDays}");
                    var extendedQuery = query.GroupBy(x => new
                    {
                        x.TypeId,
                        x.SensorId,
                        x.Sensor.Guid,
                        x.Timestamp.Year,
                        x.Timestamp.Month,
                        x.Timestamp.Day,
                        x.Timestamp.Hour
                    }).Select(x => new MeasurementExtended()
                    {
                        TypeId = x.Key.TypeId,
                        TimestampUtc = DateTime.UtcNow,
                        Timestamp = x.Max(d => d.Timestamp),
                        Value = x.Key.TypeId == (int)MeasurementTypes.Motion ? x.Max(d => d.Value) : x.Average(d => d.Value),
                        SensorId = x.Key.SensorId,
                        SensorGuid = x.Key.Guid,
                    }).OrderBy(x => x.Timestamp);
                    var rows = await extendedQuery.ToListAsync();
                    foreach (var item in rows)
                    {
                        item.TimestampUtc = _dateService.LocalToUtc(item.Timestamp);
                    }
                    return rows;
                }
                return await query.Select(x => new MeasurementExtended()
                {
                    TypeId = x.TypeId,
                    Value = x.Value,
                    Timestamp = x.Timestamp,
                    TimestampUtc = x.TimestampUtc,
                    SensorId = x.SensorId
                }).OrderBy(x => x.Timestamp).ToListAsync();
            }
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

        public async Task<List<PublicSensor>> GetPublicSensors()
        {
            return await _context.PublicSensors
                .Include(ps => ps.Sensor)
                .Include(ps => ps.MeasurementType)
                .ToListAsync();
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

        public async Task<DeviceMessage> AddDeviceMessage(DeviceMessage deviceMessage, bool saveChanges)
        {
            await _context.DeviceMessages.AddAsync(deviceMessage);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return deviceMessage;
        }

        public async Task<DeviceMessage?> GetDeviceMessage(string messageIdentifier, int deviceId)
        {
            return await _context.DeviceMessages.FirstOrDefaultAsync(x => x.Identifier == messageIdentifier && x.DeviceId == deviceId && !x.IsDuplicate);
        }
    }
}
