using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.WebApi.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class MeasurementsController : ControllerBase
    {
        private readonly IMeasurementService _measurementService;
        private readonly IDateService _dateService;
        private readonly ILogger<MeasurementsController> _logger;
        private readonly ISyncService _syncService;

        public MeasurementsController(IDateService dateService, IMeasurementService measurementService, ISyncService syncService, ILogger<MeasurementsController> logger)
        {           
            _dateService = dateService;
            _measurementService = measurementService;
            _syncService = syncService;
            _logger = logger;
        }

        [HttpGet()]
        [Authorize(Roles = "Admin, Viewer, User")]
        public async Task<MeasurementsModel> GetMeasurements([FromQuery] GetMeasurementsModel model)
        {
            var result = await _measurementService.GetMeasurements(model);
            return result;
        }

        [HttpGet("bysensor")]
        [Authorize(Roles = "Admin, Viewer, User")]
        public async Task<MeasurementsBySensorModel> GetMeasurementsBySensor([FromQuery] GetMeasurementsModel model)
        {
            return await _measurementService.GetMeasurementsBySensor(model);
        }

        [HttpGet("bylocation")]
        [Authorize(Roles = "Admin, Viewer, User")]
        public async Task<MeasurementsByLocationModel> GetMeasurementsByLocation([FromQuery] GetMeasurementsModel model)
        {
            return await _measurementService.GetMeasurementsByLocation(model);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme, Roles = "Admin,ApiKeyUser")]
        public async Task AddMeasurements([FromBody] SaveMeasurementsDto measurements)
        {
            // TODO could move this preprocessing to a service
            var enqueuedTime = DateTime.UtcNow;
            measurements.EnqueuedUtc = enqueuedTime;
            foreach (var measurement in measurements.Measurements)
            {
                measurement.TimestampUtc = enqueuedTime;
            }
            measurements.Source = CommunicationChannels.RestApi;
            await _measurementService.AddMeasurements(measurements);
        }

        [HttpPost("sync")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme, Roles = "Admin,ApiKeyUser")]
        public async Task<IActionResult> SyncMeasurements([FromBody] SyncMeasurementsRequest request)
        {
            if (request == null || request.Measurements == null || !request.Measurements.Any())
            {
                _logger.LogWarning("SyncMeasurements called with no measurements provided.");
                return BadRequest("No measurements provided");
            }

            try
            {
                var result = await _syncService.ProcessIncomingSync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing sync: {ex.Message}");
            }
        }
    }
}