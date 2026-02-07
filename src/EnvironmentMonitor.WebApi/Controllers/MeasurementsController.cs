using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.WebApi.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class MeasurementsController : ControllerBase
    {
        private readonly IMeasurementService _measurementService;
        private readonly IDateService _dateService;

        private const int PublicMeasurementMaxLimitInDays = 5;

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

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<MeasurementsBySensorModel> GetMeasurementsByPublicSensor([FromQuery] GetMeasurementsModel model)
        {
            var currentTime = _dateService.CurrentTime();
            return await _measurementService.GetMeasurementsByPublicSensor(new GetMeasurementsModel()
            {
                LatestOnly = model.LatestOnly,
                From = (currentTime - model.From).TotalDays > PublicMeasurementMaxLimitInDays ? currentTime.AddDays(-1 * PublicMeasurementMaxLimitInDays) : model.From,
                To = model.To
            });
        }

        [HttpGet("public/measurements")]
        [Authorize(Roles = "Admin, Viewer, User, Registered")]
        public async Task<MeasurementsBySensorModel> GetMeasurementsByPublicSensorFilter([FromQuery] GetMeasurementsModel model)
        {
            return await _measurementService.GetMeasurementsByPublicSensor(model);
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