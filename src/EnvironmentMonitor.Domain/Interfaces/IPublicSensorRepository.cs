using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IPublicSensorRepository
    {
        Task<List<PublicSensor>> GetPublicSensors(List<Guid>? identifiers = null);
        Task<PublicSensor?> GetPublicSensor(Guid identifier);
        Task<PublicSensor> AddPublicSensor(PublicSensor publicSensor, bool saveChanges);
        Task<PublicSensor> UpdatePublicSensor(PublicSensor publicSensor, bool saveChanges);
        Task DeletePublicSensor(PublicSensor publicSensor, bool saveChanges);
        Task SaveChanges();
    }
}
