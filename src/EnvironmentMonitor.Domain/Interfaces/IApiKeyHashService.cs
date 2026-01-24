using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IApiKeyHashService
    {
        string GenerateApiKey();
        byte[] HashApiKey(string apiKey);
        bool VerifyApiKeyHash(string apiKey, byte[] storedHash);
    }
}
