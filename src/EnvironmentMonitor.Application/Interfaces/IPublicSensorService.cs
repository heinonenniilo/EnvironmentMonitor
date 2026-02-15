using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IPublicSensorService
    {
        Task<MeasurementsBySensorModel> GetMeasurementsByPublicSensor(GetMeasurementsModel model);
        Task<List<SensorDto>> GetPublicSensors(GetPublicSensorsModel model);
        Task<List<SensorDto>> ManagePublicSensors(ManagePublicSensorsRequest request);
    }
}
