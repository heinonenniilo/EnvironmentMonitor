using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;
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
        private readonly ICacheService _cacheService;
        private readonly ILogger<ApiKeyService> _logger;
        private readonly TimeSpan _cacheExpiration;

        private const string ApiKeyCachePrefix = "apikey:";

        public ApiKeyService(
            IApiKeyRepository apiKeyRepository,
            IApiKeyHashService apiKeyHashService,
            IUserService userService,
            IMapper mapper,
            ICacheService cacheService,
            ApiKeySettings apiKeySettings,
            ILogger<ApiKeyService> logger)
        {
            _apiKeyRepository = apiKeyRepository;
            _apiKeyHashService = apiKeyHashService;
            _userService = userService;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
            _cacheExpiration = TimeSpan.FromMinutes(apiKeySettings.ApiKeyCacheExpirationMinutes);
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
            await _cacheService.RemoveAsync($"{ApiKeyCachePrefix}{id}");
            _logger.LogInformation("Deleted API key '{SecretId}' and removed from cache", id);
        }

        public async Task<ApiSecret?> VerifyApiKey(string secretId, string providedApiKey)
        {
            var secret = await _cacheService.GetAsync<ApiSecret>($"{ApiKeyCachePrefix}{secretId}");

            if (secret != null)
            {
                _logger.LogDebug("Cache hit for API key '{SecretId}'", secretId);

                if (!secret.Enabled)
                {
                    _logger.LogWarning("Cached API key '{SecretId}' is disabled", secretId);
                    return null;
                }

                return secret;
            }

            _logger.LogDebug("Cache miss for API key '{SecretId}', verifying against repository", secretId);

            secret = await _apiKeyRepository.GetApiKey(secretId);

            if (secret == null || !secret.Enabled)
            {
                _logger.LogWarning("API key '{SecretId}' not found or disabled", secretId);
                return null;
            }

            var isValid = _apiKeyHashService.VerifyApiKeyHash(providedApiKey, secret.Hash);

            if (!isValid)
            {
                _logger.LogWarning("Hash verification failed for API key '{SecretId}'", secretId);
                return null;
            }

            await _cacheService.SetAsync($"{ApiKeyCachePrefix}{secretId}", secret, _cacheExpiration);
            _logger.LogInformation("API key '{SecretId}' verified and cached with expiration of {ExpirationMinutes} minutes", secretId, _cacheExpiration.TotalMinutes);

            return secret;
        }

        public async Task<ApiKeyDto> UpdateApiKey(string id, UpdateApiKeyRequest request)
        {
            EnsureAdmin();

            var updatedSecret = await _apiKeyRepository.UpdateApiKey(id, request.Enabled, request.Description);
            await _cacheService.RemoveAsync($"{ApiKeyCachePrefix}{id}");
            _logger.LogInformation("Updated API key '{SecretId}' and invalidated cache", id);

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
