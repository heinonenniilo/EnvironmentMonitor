using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicMeasurementsController : ControllerBase
    {
        private readonly IMeasurementService _measurementService;
        private readonly IDateService _dateService;

        private static readonly int PublicMeasurementMaxLimitInDays = ApplicationConstants.PublicMeasurementMaxLimitInDays;

        public PublicMeasurementsController(IDateService dateService, IMeasurementService measurementService)
        {
            _dateService = dateService;
            _measurementService = measurementService;
        }

        [HttpGet]
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

        [HttpGet("measurements")]
        [Authorize(Roles = "Admin, Viewer, User, Registered")]
        public async Task<MeasurementsBySensorModel> GetMeasurementsByPublicSensorFilter([FromQuery] GetMeasurementsModel model)
        {
            return await _measurementService.GetMeasurementsByPublicSensor(model);
        }

        [HttpGet("sensors")]
        [Authorize(Roles = "Admin, Viewer, User, Registered")]
        public async Task<List<SensorDto>> GetPublicSensors()
        {
            return await _measurementService.GetPublicSensors();
        }
    }
}
