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
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Viewer, User")]
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
        public async Task<MeasurementsModel> GetMeasurements([FromQuery] GetMeasurementsModel model)
        {
            var result = await _measurementService.GetMeasurements(model);
            return result;
        }

        [HttpGet("bysensor")]
        public async Task<MeasurementsBySensorModel> GetMeasurementsBySensor([FromQuery] GetMeasurementsModel model)
        {
            var user = User;
            return await _measurementService.GetMeasurementsBySensor(model);
        }
    }
}