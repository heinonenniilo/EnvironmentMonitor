using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceSensorService
    {
        Task<List<SensorDto>> GetSensors(List<Guid> deviceIdentifiers);
        Task<List<SensorInfoDto>> GetSensors(Guid deviceIdentifier);
        Task<SensorDto?> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel, bool? active = true);
        Task<SensorInfoDto> AddSensor(AddOrUpdateSensorDto model);
        Task<SensorInfoDto> UpdateSensor(AddOrUpdateSensorDto model);
        Task DeleteSensor(Guid deviceIdentifier, Guid sensorIdentifier);
    }
}
