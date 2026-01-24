using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class ApiKeyHashService : IApiKeyHashService
    {
        private readonly ILogger<ApiKeyHashService> _logger;
        private readonly IDateService _dateService;

        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public ApiKeyHashService(ILogger<ApiKeyHashService> logger, IDateService dateService)
        {
            _logger = logger;
            _dateService = dateService;
        }

        public string GenerateApiKey()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }

        public byte[] HashApiKey(string apiKey)
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

        public bool VerifyApiKeyHash(string apiKey, byte[] storedHash)
        {
            if (storedHash.Length != SaltSize + HashSize)
            {
                _logger.LogWarning("Invalid stored hash length");
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

        public CreateApiSecretResult CreateApiSecret(string? description = null)
        {
            var plainApiKey = GenerateApiKey();
            var hash = HashApiKey(plainApiKey);
            var now = _dateService.CurrentTime();
            var utcNow = _dateService.LocalToUtc(now);

            var apiSecret = new ApiSecret
            {
                Id = Ulid.NewUlid().ToString(),
                Hash = hash,
                Created = now,
                CreatedUtc = utcNow,
                Description = description,
                Enabled = true
            };

            return new CreateApiSecretResult
            {
                ApiSecret = apiSecret,
                PlainKey = plainApiKey
            };
        }
    }
}
