using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IApiKeyManagementService
    {
        Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request);
        Task<List<ApiKeyDto>> GetAllApiKeys();
        Task<ApiKeyDto?> GetApiKey(string id);
        Task DeleteApiKey(string id);
    }
}
