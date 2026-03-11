using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
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
        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<List<LocationDto>> GetLocations([FromQuery] GetLocationsModel model)
            => await _locationService.GetLocations(model);

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
    }
}
