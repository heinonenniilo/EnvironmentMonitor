using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin, Viewer")]
    public class MeasurementsController : ControllerBase
    {
        private readonly IMeasurementService _measurementService;
        private readonly ILogger<MeasurementsController> _logger;

        public MeasurementsController(ILogger<MeasurementsController> logger, IMeasurementService measurementService)
        {
            _logger = logger;
            _measurementService = measurementService;
        }

        [HttpGet()]        
        public async Task<List<MeasurementDto>> GetMeasurements([FromQuery] GetMeasurementsModel model)
        {
            var result = await _measurementService.GetMeasurements(model);
            return result;
        }

        [HttpGet("bysensor")]
        public async Task<MeasurementsViewModel> GetMeasurementsBySensor([FromQuery] GetMeasurementsModel model)
        {
            return await _measurementService.GetMeasurementsBySensor(model);
        }

        [HttpGet("devices")]
        public async Task<List<DeviceDto>> GetDevices()
        {
            var result = await _measurementService.GetDevices();
            return result;
        }

        [HttpGet("sensors")]
        public async Task<List<SensorDto>> GetSensors([FromQuery] List<string> deviceIds)
        {
            var result = await _measurementService.GetSensors(deviceIds);
            return result;
        }
    }
}