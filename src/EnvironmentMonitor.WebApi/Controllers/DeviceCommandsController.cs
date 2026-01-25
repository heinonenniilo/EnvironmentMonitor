using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.WebApi.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme)]
    public class DeviceCommandsController : ControllerBase
    {
        private readonly IDeviceCommandService _deviceCommandService;
        private readonly ILogger<DeviceCommandsController> _logger;

        public DeviceCommandsController(
            IDeviceCommandService deviceCommandService,
            ILogger<DeviceCommandsController> logger)
        {
            _deviceCommandService = deviceCommandService;
            _logger = logger;
        }

        [HttpGet("{deviceIdentifier}/attributes")]
        public async Task<Dictionary<int, string>> GetDeviceAttributes([FromRoute] string deviceIdentifier) => await _deviceCommandService.GetDeviceAttributes(deviceIdentifier);
    }
}
