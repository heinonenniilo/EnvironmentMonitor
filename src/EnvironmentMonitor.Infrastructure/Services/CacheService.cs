using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly ConcurrentDictionary<string, bool> _keys = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var data = await _cache.GetStringAsync(key);

            if (data == null)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(data, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached value for key '{Key}'", key);
                await _cache.RemoveAsync(key);
                _keys.TryRemove(key, out _);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            var data = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.SetStringAsync(key, data, options);
            _keys.TryAdd(key, true);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
            _keys.TryRemove(key, out _);
        }

        public async Task ClearAsync()
        {
            foreach (var key in _keys.Keys)
            {
                await _cache.RemoveAsync(key);
            }
            _keys.Clear();
        }
    }
}
