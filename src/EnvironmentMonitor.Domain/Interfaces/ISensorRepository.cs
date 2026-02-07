using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface ISensorRepository
    {
        Task<Sensor?> GetSensor(Guid identifier);
        Task<Sensor?> GetSensor(int id);
        Task<List<Sensor>> GetSensorsByDevice(int deviceId, bool? isActive = null);
        Task<Sensor> AddSensor(Sensor sensor, bool saveChanges);
        Task<Sensor> UpdateSensor(Sensor sensor, bool saveChanges);
        Task DeleteSensor(Guid identifier, bool saveChanges);
        Task SaveChanges();
    }
}
