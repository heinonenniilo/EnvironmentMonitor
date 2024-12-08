using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Data.Migrations.Application;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.WebApi.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {

        private readonly IDeviceService _deviceService;
        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost("reboot")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reboot([FromBody] MessageDeviceModel model)
        {
            await _deviceService.Reboot(model.DeviceIdentifier);
            return Ok(new { Message = "Message sent" });
        }
    }
}
