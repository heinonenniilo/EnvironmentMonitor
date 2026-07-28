using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class StatusVariableRepository : IStatusVariableRepository
    {
        private readonly MeasurementDbContext _context;

        public StatusVariableRepository(MeasurementDbContext context)
        {
            _context = context;
        }

        public async Task<StatusVariable?> GetByKey(string key)
        {
            return await _context.StatusVariables
                .FirstOrDefaultAsync(sv => sv.Key == key);
        }

        public async Task SetValue(string key, string value)
        {
            var existing = await GetByKey(key);

            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedUtc = DateTime.UtcNow;
            }
            else
            {
                var newVariable = new StatusVariable
                {
                    Key = key,
                    Value = value,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow
                };
                _context.StatusVariables.Add(newVariable);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<DeviceMessage>> GetUnsyncedDeviceMessages(long lastSyncedId, int batchSize)
        {
            return await _context.DeviceMessages
                .Include(dm => dm.Device)
                .Include(dm => dm.Measurements)
                    .ThenInclude(m => m.Sensor)
                .Where(dm => dm.Id > lastSyncedId && !dm.IsDuplicate)
                .OrderBy(dm => dm.Id)
                .Take(batchSize)
                .ToListAsync();
        }
    }
}
