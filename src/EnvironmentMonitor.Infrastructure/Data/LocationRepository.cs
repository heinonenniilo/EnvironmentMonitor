using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models.GetModels;
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
        public async Task<List<Location>> GetLocations(GetLocationsModel model)
        {
            IQueryable<Location> query = _context.Locations;
            if (model.Ids != null)
            {
                query = query.Where(x => model.Ids.Contains(x.Id));
            }
            if (model.IncludeLocationSensors)
            {
                query = query.Include(x => x.LocationSensors).ThenInclude(ls => ls.Sensor);
            }

            if (model.Visible != null)
            {
                query = query.Where(x => x.Visible == model.Visible.Value);
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
