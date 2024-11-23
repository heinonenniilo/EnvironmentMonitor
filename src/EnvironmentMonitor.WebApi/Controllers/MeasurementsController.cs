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
        [Authorize(Roles = "Admin, Viewer") ]
        public async Task<List<MeasurementDto>> Register([FromQuery] GetMeasurementsModel model)
        {
            var result = await _measurementService.GetMeasurements(model);
            return result;
        }
    }
}
