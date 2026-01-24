using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IApiKeyRepository
    {
        Task<ApiSecret> AddApiKey(ApiSecret apiSecret, bool saveChanges = true);
        Task<List<ApiSecret>> GetAllApiKeys();
        Task<ApiSecret?> GetApiKey(string id);
        Task DeleteApiKey(string id, bool saveChanges = true);
        Task<ApiSecret> UpdateApiKey(string id, bool? enabled, string? description, bool saveChanges = true);
    }
}
