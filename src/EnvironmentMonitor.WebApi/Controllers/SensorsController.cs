using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SensorsController : ControllerBase
    {
        private readonly IDeviceSensorService _deviceSensorService;

        public SensorsController(IDeviceSensorService deviceSensorService)
        {
            _deviceSensorService = deviceSensorService;
        }

        [HttpGet("{deviceIdentifier}")]
        public async Task<List<SensorInfoDto>> GetSensors([FromRoute] Guid deviceIdentifier)
            => await _deviceSensorService.GetSensors(deviceIdentifier);

        [HttpPost]
        public async Task<SensorInfoDto> AddSensor([FromBody] AddOrUpdateSensorDto model)
            => await _deviceSensorService.AddSensor(model);

        [HttpPut]
        public async Task<SensorInfoDto> UpdateSensor([FromBody] AddOrUpdateSensorDto model)
            => await _deviceSensorService.UpdateSensor(model);

        [HttpDelete("{deviceIdentifier}/{sensorIdentifier}")]
        public async Task DeleteSensor([FromRoute] Guid deviceIdentifier, [FromRoute] Guid sensorIdentifier)
            => await _deviceSensorService.DeleteSensor(deviceIdentifier, sensorIdentifier);
    }
}
