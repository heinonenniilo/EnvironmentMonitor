using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
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
        private readonly ILocationCommandService _locationCommandService;
        private readonly ILogger<DeviceCommandsController> _logger;

        public DeviceCommandsController(
            IDeviceCommandService deviceCommandService,
            ILocationCommandService locationCommandService,
            ILogger<DeviceCommandsController> logger)
        {
            _deviceCommandService = deviceCommandService;
            _locationCommandService = locationCommandService;
            _logger = logger;
        }

        [HttpGet("{deviceIdentifier}/attributes")]
        public async Task<Dictionary<int, string>> GetDeviceAttributes([FromRoute] string deviceIdentifier) => await _deviceCommandService.GetDeviceAttributes(deviceIdentifier);

        [HttpPost("location/{locationIdentifier}/motion-control-status")]
        [Authorize(Roles = "Admin")]
        public async Task SetLocationMotionControlStatus([FromRoute] Guid locationIdentifier, [FromBody] SetMotionControlStatusMessage model)
            => await _locationCommandService.SetMotionControlStatus(locationIdentifier, (MotionControlStatus)model.Mode, model.ExecuteAt);

        [HttpPost("location/{locationIdentifier}/motion-control-delay")]
        [Authorize(Roles = "Admin")]
        public async Task SetLocationMotionControlDelay([FromRoute] Guid locationIdentifier, [FromBody] SetMotionControlDelayMessag model)
            => await _locationCommandService.SetMotionControlDelay(locationIdentifier, model.DelayMs, model.ExecuteAt);
    }
}
