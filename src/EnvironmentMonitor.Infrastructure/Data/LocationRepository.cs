using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Exceptions;
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

            if (model.Identifiers != null)
            {
                query = query.Where(x => model.Identifiers.Contains(x.Identifier));
            }

            if (model.IncludeLocationSensors)
            {
                query = query.Include(x => x.LocationSensors).ThenInclude(ls => ls.Sensor);
            }

            if (model.GetDevices)
            {
                query = query.Include(x => x.Devices);
            }

            if (model.Visible != null)
            {
                query = query.Where(x => x.Visible == model.Visible.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<LocationSensor>> GetLocationSensors(List<int> locationIds)
        {
            return await _context.LocationSensors.Where(x => locationIds.Contains(x.LocationId)).ToListAsync();
        }

        public async Task<Location> AddLocation(Location location, bool saveChanges)
        {
            _context.Locations.Add(location);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return location;
        }

        public async Task DeleteLocation(int locationId, bool saveChanges)
        {
            var location = await _context.Locations.FindAsync(locationId)
                ?? throw new EntityNotFoundException($"Location with id: {locationId} not found.");

            _context.Locations.Remove(location);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<LocationSensor> AddLocationSensor(int locationId, int sensorId, int deviceId, string name, int? typeId, bool saveChanges)
        {
            var location = await _context.Locations.FindAsync(locationId)
                ?? throw new EntityNotFoundException($"Location with id: {locationId} not found.");
            var sensor = await _context.Sensors.Include(s => s.Device).FirstOrDefaultAsync(s => s.Id == sensorId && s.DeviceId == deviceId)
                ?? throw new EntityNotFoundException($"Sensor with id: {sensorId} and deviceId: {deviceId} not found.");
            var device = sensor.Device
                ?? throw new EntityNotFoundException($"Device with id: {deviceId} not found.");

            var locationSensor = new LocationSensor
            {
                LocationId = locationId,
                SensorId = sensorId,
                DeviceId = deviceId,
                Name = name,
                TypeId = typeId,
                Location = location,
                Sensor = sensor,
                Device = device,
            };

            _context.LocationSensors.Add(locationSensor);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return locationSensor;
        }

        public async Task<LocationSensor> UpdateLocationSensor(int locationId, int sensorId, int deviceId, string name, int? typeId, bool saveChanges)
        {
            var existing = await _context.LocationSensors
                .FirstOrDefaultAsync(x => x.LocationId == locationId && x.SensorId == sensorId && x.DeviceId == deviceId)
                ?? throw new EntityNotFoundException($"LocationSensor not found for LocationId: {locationId}, SensorId: {sensorId}, DeviceId: {deviceId}.");

            existing.Name = name;
            existing.TypeId = typeId;

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return existing;
        }

        public async Task DeleteLocationSensor(int locationId, int sensorId, int deviceId, bool saveChanges)
        {
            var existing = await _context.LocationSensors
                .FirstOrDefaultAsync(x => x.LocationId == locationId && x.SensorId == sensorId && x.DeviceId == deviceId)
                ?? throw new EntityNotFoundException($"LocationSensor not found for LocationId: {locationId}, SensorId: {sensorId}, DeviceId: {deviceId}.");

            _context.LocationSensors.Remove(existing);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task MoveDevicesToLocation(int locationId, List<int> deviceIds, bool saveChanges)
        {
            var location = await _context.Locations.FindAsync(locationId)
                ?? throw new EntityNotFoundException($"Location with id: {locationId} not found.");

            var devices = await _context.Devices.Where(x => deviceIds.Contains(x.Id)).ToListAsync();
            if (devices.Count != deviceIds.Count)
            {
                throw new EntityNotFoundException("One or more devices not found.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.LocationSensors
                    .Where(x => deviceIds.Contains(x.DeviceId))
                    .ExecuteDeleteAsync();

                await _context.Devices
                    .Where(x => deviceIds.Contains(x.Id))
                    .ExecuteUpdateAsync(s => s.SetProperty(d => d.LocationId, locationId));

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Location> UpdateLocation(Location location, bool saveChanges)
        {
            var existing = await _context.Locations.FirstOrDefaultAsync(x => x.Id == location.Id)
                ?? throw new EntityNotFoundException($"Location with id: {location.Id} not found.");

            existing.Name = location.Name;
            existing.Visible = location.Visible;

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return existing;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
