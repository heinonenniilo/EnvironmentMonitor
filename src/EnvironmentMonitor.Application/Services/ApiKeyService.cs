using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IApiKeyHashService _apiKeyHashService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ApiKeyService(
            IApiKeyRepository apiKeyRepository,
            IApiKeyHashService apiKeyHashService,
            IUserService userService,
            IMapper mapper)
        {
            _apiKeyRepository = apiKeyRepository;
            _apiKeyHashService = apiKeyHashService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request)
        {
            EnsureAdmin();

            var result = _apiKeyHashService.CreateApiSecret(request.Description);

            var claims = new List<SecretClaim>();

            if (request.DeviceIds?.Any() == true)
            {
                foreach (var deviceId in request.DeviceIds)
                {
                    claims.Add(new SecretClaim
                    {
                        Type = EntityRoles.Device.ToString(),
                        Value = deviceId.ToString(),
                        ApiSecretId = result.ApiSecret.Id
                    });
                }
            }

            if (request.LocationIds?.Any() == true)
            {
                foreach (var locationId in request.LocationIds)
                {
                    claims.Add(new SecretClaim
                    {
                        Type = EntityRoles.Location.ToString(),
                        Value = locationId.ToString(),
                        ApiSecretId = result.ApiSecret.Id
                    });
                }
            }

            result.ApiSecret.Claims = claims;
            
            var createdSecret = await _apiKeyRepository.AddApiKey(result.ApiSecret);

            return new CreateApiKeyResponse
            {
                ApiKey = result.PlainKey,
                Id = createdSecret.Id,
                Description = createdSecret.Description,
                Created = createdSecret.Created
            };
        }

        public async Task<List<ApiKeyDto>> GetAllApiKeys()
        {
            EnsureAdmin();

            var secrets = await _apiKeyRepository.GetAllApiKeys();

            return _mapper.Map<List<ApiKeyDto>>(secrets);
        }

        public async Task<ApiKeyDto?> GetApiKey(string id)
        {
            EnsureAdmin();

            var secret = await _apiKeyRepository.GetApiKey(id);
            
            if (secret == null)
            {
                return null;
            }

            return _mapper.Map<ApiKeyDto>(secret);
        }

        public async Task DeleteApiKey(string id)
        {
            EnsureAdmin();

            await _apiKeyRepository.DeleteApiKey(id);
        }

        public async Task<ApiSecret?> VerifyApiKey(string secretId, string providedApiKey)
        {
            var secret = await _apiKeyRepository.GetApiKey(secretId);

            if (secret == null || !secret.Enabled)
            {
                return null;
            }

            var isValid = _apiKeyHashService.VerifyApiKeyHash(providedApiKey, secret.Hash);
            
            if (!isValid)
            {
                return null;
            }

            return secret;
        }

        public async Task<ApiKeyDto> UpdateApiKey(string id, UpdateApiKeyRequest request)
        {
            EnsureAdmin();

            var updatedSecret = await _apiKeyRepository.UpdateApiKey(id, request.Enabled, request.Description);

            return _mapper.Map<ApiKeyDto>(updatedSecret);
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
