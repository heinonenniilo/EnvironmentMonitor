using EnvironmentMonitor.Application.DTOs;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceSensorService
    {
        Task<List<SensorInfoDto>> GetSensors(Guid deviceIdentifier);
        Task<SensorInfoDto> AddSensor(AddOrUpdateSensorDto model);
        Task<SensorInfoDto> UpdateSensor(AddOrUpdateSensorDto model);
        Task DeleteSensor(Guid deviceIdentifier, Guid sensorIdentifier);
    }
}
