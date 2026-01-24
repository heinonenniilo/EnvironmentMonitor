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
        private readonly IDateService _dateService;
        private readonly IMapper _mapper;

        public ApiKeyService(
            IApiKeyRepository apiKeyRepository,
            IApiKeyHashService apiKeyHashService,
            IUserService userService,
            IDateService dateService,
            IMapper mapper)
        {
            _apiKeyRepository = apiKeyRepository;
            _apiKeyHashService = apiKeyHashService;
            _userService = userService;
            _dateService = dateService;
            _mapper = mapper;
        }

        public async Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request)
        {
            EnsureAdmin();

            var plainApiKey = _apiKeyHashService.GenerateApiKey();
            var hash = _apiKeyHashService.HashApiKey(plainApiKey);
            var now = _dateService.CurrentTime();
            var utcNow = _dateService.LocalToUtc(now);

            var apiSecret = new ApiSecret
            {
                Id = Guid.NewGuid().ToString(),
                Hash = hash,
                Created = now,
                CreatedUtc = utcNow,
                Description = request.Description
            };

            var claims = new List<SecretClaim>();

            if (request.DeviceIds?.Any() == true)
            {
                foreach (var deviceId in request.DeviceIds)
                {
                    claims.Add(new SecretClaim
                    {
                        Type = EntityRoles.Device.ToString(),
                        Value = deviceId.ToString(),
                        ApiSecretId = apiSecret.Id
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
                        ApiSecretId = apiSecret.Id
                    });
                }
            }

            apiSecret.Claims = claims;
            
            var createdSecret = await _apiKeyRepository.AddApiKey(apiSecret);

            return new CreateApiKeyResponse
            {
                ApiKey = plainApiKey,
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

        private void EnsureAdmin()
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
