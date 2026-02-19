using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.GetModels;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IPublicSensorRepository
    {
        Task<List<PublicSensor>> GetPublicSensors(GetPublicSensorsModel model);
        Task<PublicSensor?> GetPublicSensor(Guid identifier);
        Task<PublicSensor> AddPublicSensor(PublicSensor publicSensor, bool saveChanges);
        Task<PublicSensor> UpdatePublicSensor(PublicSensor publicSensor, bool saveChanges);
        Task DeletePublicSensor(PublicSensor publicSensor, bool saveChanges);
        Task SaveChanges();
    }
}
