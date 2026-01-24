using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IApiKeyRepository
    {
        Task<(ApiSecret Secret, string PlainKey)> CreateApiKey(List<Guid> deviceIds, List<Guid> locationIds, string? description);
        Task<List<ApiSecret>> GetAllApiKeys();
        Task<ApiSecret?> GetApiKey(string id);
        Task DeleteApiKey(string id);
        Task<bool> VerifyApiKey(string secretId, string providedApiKey);
    }
}
