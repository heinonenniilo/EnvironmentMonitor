using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeysController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpPost]
        public async Task<CreateApiKeyResponse> CreateApiKey([FromBody] CreateApiKeyRequest request) 
            => await _apiKeyService.CreateApiKey(request);

        [HttpGet]
        public async Task<List<ApiKeyDto>> GetAllApiKeys() 
            => await _apiKeyService.GetAllApiKeys();

        [HttpGet("{id}")]
        public async Task<ApiKeyDto?> GetApiKey(string id) 
            => await _apiKeyService.GetApiKey(id);

        [HttpDelete("{id}")]
        public async Task DeleteApiKey(string id) 
            => await _apiKeyService.DeleteApiKey(id);

        [HttpPatch("{id}")]
        public async Task<ApiKeyDto> UpdateApiKey(string id, [FromBody] UpdateApiKeyRequest request) 
            => await _apiKeyService.UpdateApiKey(id, request);
    }
}
