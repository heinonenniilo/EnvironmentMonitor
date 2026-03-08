using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.GetModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface ILocationRepository
    {
        public Task<List<LocationSensor>> GetLocationSensors(List<int> locationIds);
        public Task<List<Location>> GetLocations(GetLocationsModel model);
        public Task<Location> AddLocation(Location location, bool saveChanges);
        public Task DeleteLocation(int locationId, bool saveChanges);
        public Task<LocationSensor> AddLocationSensor(int locationId, int sensorId, int deviceId, string name, int? typeId, bool saveChanges);
        public Task<LocationSensor> UpdateLocationSensor(int locationId, int sensorId, int deviceId, string name, int? typeId, bool saveChanges);
        public Task DeleteLocationSensor(int locationId, int sensorId, int deviceId, bool saveChanges);
        public Task MoveDevicesToLocation(int locationId, List<int> deviceIds, bool saveChanges);
        public Task SaveChanges();
    }
}
