using EnvironmentMonitor.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface ILocationService
    {
        public Task<List<LocationDto>> GetLocations(bool getDevices = false);
        public Task<LocationDto> AddLocation(AddLocationDto model);
        public Task DeleteLocation(Guid locationIdentifier);
        public Task<LocationDto> AddLocationSensor(AddOrUpdateLocationSensorDto model);
        public Task<LocationDto> UpdateLocationSensor(AddOrUpdateLocationSensorDto model);
        public Task<LocationDto> DeleteLocationSensor(AddOrUpdateLocationSensorDto model);
        public Task MoveDevicesToLocation(MoveDevicesToLocationDto model);
    }
}
