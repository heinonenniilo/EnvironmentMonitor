using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class LocationRepository : ILocationRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly IDateService _dateService;
        public LocationRepository(MeasurementDbContext context, IDateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }
        public async Task<List<Location>> GetLocations(List<int>? ids, bool includeLocationSensors)
        {
            IQueryable<Location> query = _context.Locations;
            if (ids != null)
            {
                query = query.Where(x => ids.Contains(x.Id));
            }

            if (includeLocationSensors)
            {
                query = query.Include(x => x.LocationSensors);
            }
            query = query.Where(x => x.Id > 0);
            return await query.ToListAsync();
        }

        public async Task<List<LocationSensor>> GetLocationSensors(List<int> locationIds)
        {
            return await _context.LocationSensors.Where(x => locationIds.Contains(x.LocationId)).ToListAsync();
        }
    }
}
