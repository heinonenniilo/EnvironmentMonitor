using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class ApiKeyManagementService : IApiKeyManagementService
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ApiKeyManagementService(IApiKeyService apiKeyService, IUserService userService, IMapper mapper)
        {
            _apiKeyService = apiKeyService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request)
        {
            EnsureAdmin();

            var (secret, plainKey) = await _apiKeyService.CreateApiKey(
                request.DeviceIds,
                request.LocationIds,
                request.Description);

            return new CreateApiKeyResponse
            {
                ApiKey = plainKey,
                Id = secret.Id,
                Description = secret.Description,
                Created = secret.Created
            };
        }

        public async Task<List<ApiKeyDto>> GetAllApiKeys()
        {
            EnsureAdmin();

            var secrets = await _apiKeyService.GetAllApiKeys();

            return _mapper.Map<List<ApiKeyDto>>(secrets);
        }

        public async Task<ApiKeyDto?> GetApiKey(string id)
        {
            EnsureAdmin();

            var secret = await _apiKeyService.GetApiKey(id);
            
            if (secret == null)
            {
                return null;
            }

            return _mapper.Map<ApiKeyDto>(secret);
        }

        public async Task DeleteApiKey(string id)
        {
            EnsureAdmin();

            await _apiKeyService.DeleteApiKey(id);
        }

        private void EnsureAdmin()
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
