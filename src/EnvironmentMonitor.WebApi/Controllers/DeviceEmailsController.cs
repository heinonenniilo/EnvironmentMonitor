using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DeviceEmailsController : ControllerBase
    {
        private readonly IDeviceEmailService _deviceEmailService;

        public DeviceEmailsController(IDeviceEmailService deviceEmailService)
        {
            _deviceEmailService = deviceEmailService;
        }

        [HttpGet("templates")]
        public async Task<List<DeviceEmailTemplateDto>> GetAllEmailTemplates() => await _deviceEmailService.GetAllEmailTemplates();

        [HttpPut]
        public async Task<DeviceEmailTemplateDto> UpdateEmailTemplate([FromBody] UpdateDeviceEmailTemplateDto model) => await _deviceEmailService.UpdateEmailTemplate(model);
    }
}
