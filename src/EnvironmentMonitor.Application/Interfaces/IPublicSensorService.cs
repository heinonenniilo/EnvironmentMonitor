using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IPublicSensorService
    {
        Task<MeasurementsBySensorModel> GetMeasurementsByPublicSensor(GetMeasurementsModel model);
        Task<List<SensorDto>> GetPublicSensors();
        Task<List<SensorDto>> ManagePublicSensors(ManagePublicSensorsRequest request);
    }
}
