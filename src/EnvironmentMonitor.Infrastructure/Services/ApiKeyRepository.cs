using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiKeyRepository> _logger;
        private readonly IDateService _dateService;

        public ApiKeyRepository(
            ApplicationDbContext context,
            ILogger<ApiKeyRepository> logger,
            IDateService dateService)
        {
            _context = context;
            _logger = logger;
            _dateService = dateService;
        }

        public async Task<ApiSecret> AddApiKey(ApiSecret apiSecret, bool saveChanges = true)
        {
            _context.ApiSecrets.Add(apiSecret);
            
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Created API key with ID: {apiSecret.Id}");

            return apiSecret;
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

        public async Task DeleteApiKey(string id, bool saveChanges = true)
        {
            var secret = await _context.ApiSecrets
                .Include(s => s.Claims)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (secret == null)
            {
                throw new InvalidOperationException($"API key with ID {id} not found");
            }

            _context.ApiSecrets.Remove(secret);
            
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Deleted API key with ID: {id}");
        }

        public async Task<ApiSecret> UpdateApiKey(string id, bool? enabled, string? description, bool saveChanges = true)
        {
            var secret = await _context.ApiSecrets
                .Include(s => s.Claims)
                .FirstOrDefaultAsync(s => s.Id == id);

            var changesMade = false;

            if (secret == null)
            {
                throw new InvalidOperationException($"API key with ID {id} not found");
            }

            if (enabled.HasValue)
            {
                secret.Enabled = enabled.Value;
                changesMade = true;
            }

            if (description != null)
            {
                secret.Description = description;
                changesMade = true;
            }

            if (changesMade)
            {
                var currentTime = _dateService.CurrentTime();
                secret.Updated = currentTime;
                secret.UpdatedUtc = _dateService.LocalToUtc(currentTime);
            }

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Updated API key with ID: {id}");

            return secret;
        }
    }
}
