using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IApiKeyService
    {
        Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request);
        Task<List<ApiKeyDto>> GetAllApiKeys();
        Task<ApiKeyDto?> GetApiKey(string id);
        Task DeleteApiKey(string id);
        Task<ApiSecret?> VerifyApiKey(string secretId, string providedApiKey);
        Task<ApiKeyDto> UpdateApiKey(string id, UpdateApiKeyRequest request);
    }
}
