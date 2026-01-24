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

        public ApiKeyRepository(
            ApplicationDbContext context,
            ILogger<ApiKeyRepository> logger)
        {
            _context = context;
            _logger = logger;
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
    }
}
