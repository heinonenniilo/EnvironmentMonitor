using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDateService _dateService;
        private readonly ILogger<ApiKeyService> _logger;

        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public ApiKeyService(
            ApplicationDbContext context,
            IDateService dateService,
            ILogger<ApiKeyService> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
        }

        public async Task<(ApiSecret Secret, string PlainKey)> CreateApiKey(List<Guid> deviceIds, List<Guid> locationIds, string? description)
        {
            var plainApiKey = GenerateApiKey();
            var hash = HashApiKey(plainApiKey);
            var now = _dateService.CurrentTime();
            var utcNow = _dateService.LocalToUtc(now);

            var apiSecret = new ApiSecret
            {
                Id = Guid.NewGuid().ToString(),
                Hash = hash,
                Created = now,
                CreatedUtc = utcNow,
                Description = description
            };

            var claims = new List<SecretClaim>();

            if (deviceIds.Any())
            {
                foreach (var deviceId in deviceIds)
                {
                    claims.Add(new SecretClaim
                    {
                        Type = EntityRoles.Device.ToString(),
                        Value = deviceId.ToString(),
                        ApiSecretId = apiSecret.Id
                    });
                }
            }

            if (locationIds.Any())
            {
                foreach (var locationId in locationIds)
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
            _context.ApiSecrets.Add(apiSecret);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created API key with ID: {apiSecret.Id}");

            return (apiSecret, plainApiKey);
        }

        public async Task<List<ApiSecret>> GetAllApiKeys()
        {
            return await _context.ApiSecrets
                .Include(s => s.Claims)
                .OrderByDescending(s => s.Created)
                .ToListAsync();
        }

        public async Task<ApiSecret?> GetApiKey(string id)
        {
            return await _context.ApiSecrets
                .Include(s => s.Claims)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task DeleteApiKey(string id)
        {
            var secret = await _context.ApiSecrets
                .Include(s => s.Claims)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (secret == null)
            {
                throw new InvalidOperationException($"API key with ID {id} not found");
            }

            _context.ApiSecrets.Remove(secret);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted API key with ID: {id}");
        }

        public async Task<bool> VerifyApiKey(string secretId, string providedApiKey)
        {
            var secret = await _context.ApiSecrets
                .FirstOrDefaultAsync(s => s.Id == secretId);

            if (secret == null)
            {
                return false;
            }

            return VerifyApiKeyHash(providedApiKey, secret.Hash);
        }

        private string GenerateApiKey()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }

        private byte[] HashApiKey(string apiKey)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(apiKey, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return hashBytes;
        }

        private bool VerifyApiKeyHash(string apiKey, byte[] storedHash)
        {
            if (storedHash.Length != SaltSize + HashSize)
            {
                return false;
            }

            var salt = new byte[SaltSize];
            Array.Copy(storedHash, 0, salt, 0, SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(apiKey, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (storedHash[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
