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

        public MeasurementsController(IDateService dateService, IMeasurementService measurementService)
        {           
            _dateService = dateService;
            _measurementService = measurementService;
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
    }
}