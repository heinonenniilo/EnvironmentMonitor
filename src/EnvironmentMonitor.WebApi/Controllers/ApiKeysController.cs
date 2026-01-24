using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyManagementService;
        private readonly ILogger<ApiKeysController> _logger;

        public ApiKeysController(IApiKeyService apiKeyManagementService, ILogger<ApiKeysController> logger)
        {
            _apiKeyManagementService = apiKeyManagementService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CreateApiKeyResponse>> CreateApiKey([FromBody] CreateApiKeyRequest request)
        {
            try
            {
                var response = await _apiKeyManagementService.CreateApiKey(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key");
                return StatusCode(500, new { Message = "Failed to create API key" });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ApiKeyDto>>> GetAllApiKeys()
        {
            try
            {
                var apiKeys = await _apiKeyManagementService.GetAllApiKeys();
                return Ok(apiKeys);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching API keys");
                return StatusCode(500, new { Message = "Failed to fetch API keys" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiKeyDto>> GetApiKey(string id)
        {
            try
            {
                var apiKey = await _apiKeyManagementService.GetApiKey(id);
                if (apiKey == null)
                {
                    return NotFound(new { Message = $"API key with ID {id} not found" });
                }
                return Ok(apiKey);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching API key");
                return StatusCode(500, new { Message = "Failed to fetch API key" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteApiKey(string id)
        {
            try
            {
                await _apiKeyManagementService.DeleteApiKey(id);
                return Ok(new { Message = "API key deleted successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting API key");
                return StatusCode(500, new { Message = "Failed to delete API key" });
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ApiKeyDto>> UpdateApiKey(string id, [FromBody] UpdateApiKeyRequest request)
        {
            try
            {
                var updatedApiKey = await _apiKeyManagementService.UpdateApiKey(id, request);
                return Ok(updatedApiKey);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API key");
                return StatusCode(500, new { Message = "Failed to update API key" });
            }
        }
    }
}
