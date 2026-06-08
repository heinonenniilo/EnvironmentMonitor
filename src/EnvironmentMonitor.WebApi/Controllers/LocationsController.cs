using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Application.Services;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User, Viewer, Admin")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILocationCommandService _locationCommandService;

        public LocationsController(ILocationService locationService, ILocationCommandService locationCommandService)
        {
            _locationService = locationService;
            _locationCommandService = locationCommandService;
        }

        [HttpGet]
        public async Task<List<LocationDto>> GetLocations([FromQuery] GetLocationsModel model)
        {
            return await _locationService.GetLocations(new GetLocationsModel()
            {
                GetDevices = false,
                Identifiers = model.Identifiers,
                IncludeLocationSensors = true
            });
        }

        [HttpGet("{locationIdentifier}")]
        public async Task<LocationDto> GetLocation([FromRoute] Guid locationIdentifier)
        {
            var locations = await _locationService.GetLocations(new GetLocationsModel()
            {
                Identifiers = [locationIdentifier],
                GetDevices = true,
                IncludeLocationSensors = true
            });
            return locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{locationIdentifier}' not found.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<LocationDto> AddLocation([FromBody] AddLocationDto model) => await _locationService.AddLocation(model);

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<LocationDto> UpdateLocation([FromBody] LocationDto model) => await _locationService.UpdateLocation(model);

        [HttpDelete("{locationIdentifier}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLocation(Guid locationIdentifier)
        {
            await _locationService.DeleteLocation(locationIdentifier);
            return Ok();
        }

        [HttpPost("sensors")]
        [Authorize(Roles = "Admin")]
        public async Task<LocationDto> AddLocationSensor([FromBody] AddOrUpdateLocationSensorDto model) => await _locationService.AddLocationSensor(model);

        [HttpPut("sensors")]
        [Authorize(Roles = "Admin")]
        public async Task<LocationDto> UpdateLocationSensor([FromBody] AddOrUpdateLocationSensorDto model) => await _locationService.UpdateLocationSensor(model);

        [HttpDelete("{locationIdentifier}/sensors/{sensorIdentifier}")]
        [Authorize(Roles = "Admin")]
        public async Task<LocationDto> DeleteLocationSensor(Guid locationIdentifier, Guid sensorIdentifier)
            => await _locationService.DeleteLocationSensor(new AddOrUpdateLocationSensorDto
            {
                LocationIdentifier = locationIdentifier,
                SensorIdentifier = sensorIdentifier,
                Name = string.Empty
            });

        [HttpPost("move-devices")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MoveDevicesToLocation([FromBody] MoveDevicesToLocationDto model)
        {
            await _locationService.MoveDevicesToLocation(model);
            return Ok();
        }

        [HttpPost("{locationIdentifier}/motion-control-status")]
        [Authorize(Roles = "Admin")]
        public async Task SetLocationMotionControlStatus([FromRoute] Guid locationIdentifier, [FromBody] SetMotionControlStatusMessage model)
            => await _locationCommandService.SetMotionControlStatus(locationIdentifier, (MotionControlStatus)model.Mode, model.ExecuteAt);

        [HttpPost("{locationIdentifier}/motion-control-delay")]
        [Authorize(Roles = "Admin")]
        public async Task SetLocationMotionControlDelay([FromRoute] Guid locationIdentifier, [FromBody] SetMotionControlDelayMessag model)
            => await _locationCommandService.SetMotionControlDelay(locationIdentifier, model.DelayMs, model.ExecuteAt);
    }
}
