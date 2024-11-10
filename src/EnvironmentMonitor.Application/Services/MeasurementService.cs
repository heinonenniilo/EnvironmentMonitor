using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Application.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly IMeasurementRepository _measurementRepository;
        private readonly ILogger<MeasurementService> _logger;

        public MeasurementService(IMeasurementRepository measurement, ILogger<MeasurementService> logger)
        {
            _measurementRepository = measurement;
            _logger = logger;
        }

        public async Task AddMeasurements(MeasurementDto measurent)
        {
            var device = await _measurementRepository.GetDeviceByIdAsync(measurent.DeviceId);
            if (device == null)
            {
                _logger.LogInformation($"Could not find device with device id '{measurent.DeviceId}'");
                return;
            }
            _logger.LogInformation($"Found device with ID: {device.Id}");
            var measurementsToAdd = new List<Measurement>();
            foreach (var row in measurent.Measurements)
            {
                var sensor = await _measurementRepository.GetSensor(device.Id, row.SensorId);
                if (sensor != null)
                {
                    _logger.LogInformation($"Found sensonr with id '{sensor.Id}'.");
                    measurementsToAdd.Add(new Measurement()
                    {
                        SensorId = sensor.Id,
                        Value = row.SensorValue,
                        Timestamp = row.TimeStamp,
                    });
                }
                else
                {
                    _logger.LogWarning($"Could not find a sensor with device id: '{device.Id}' and sensor id: '{row.SensorId}'");
                }
            }

            if (measurementsToAdd.Any())
            {
                await _measurementRepository.AddMeasurements(measurementsToAdd);
            }
            return;
        }
    }
}
