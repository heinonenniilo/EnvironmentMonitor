using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
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

        public async Task AddMeasurements(SaveMeasurementsDto measurent)
        {
            var device = await _measurementRepository.GetDeviceByIdAsync(measurent.DeviceId);
            if (device == null)
            {
                _logger.LogInformation($"Could not find device with device id '{measurent.DeviceId}'");
                return;
            }
            _logger.LogInformation($"Found device with ID: {device.Id} for device id '{measurent.DeviceId}'");
            var measurementsToAdd = new List<Measurement>();
            foreach (var row in measurent.Measurements)
            {
                var sensor = await _measurementRepository.GetSensor(device.Id, row.SensorId);
                MeasurementType? type = await _measurementRepository.GetMeasurementType(row.TypeId);
                if (type == null)
                {
                    _logger.LogWarning($"Measurement type id ({row.TypeId}) not found. ");
                    continue;
                }
                _logger.LogInformation($"Found measurement type: {type.Id}. Name: '{type.Name}'");
                if (sensor == null)
                {
                    _logger.LogWarning($"Could not find a sensor with device id: '{device.Id}' and sensor id: '{row.SensorId}'");
                    continue;
                }
                _logger.LogInformation($"Found sensor: {sensor.Id}. Name: '{sensor.Name}'");
                measurementsToAdd.Add(new Measurement()
                {
                    SensorId = sensor.Id,
                    Value = row.SensorValue,
                    Timestamp = row.TimeStamp,
                    TypeId = row.TypeId
                });
            }
            if (measurementsToAdd.Any())
            {
                _logger.LogInformation($"Adding {measurementsToAdd.Count} measurements for Device: {device.Id} / '{device.Name}'");
                await _measurementRepository.AddMeasurements(measurementsToAdd);
                _logger.LogInformation("Measurements added");
            }
            else
            {
                _logger.LogWarning("No measurements to add");
            }
        }
        public async Task<List<MeasurementDto>> GetMeasurements(GetMeasurementsModel model)
        {
            var rows = await _measurementRepository.GetMeasurements(model);
            return rows.Select(x => new MeasurementDto()
            {
                SensorId = x.SensorId,
                SensorValue = x.Value,
                TypeId = x.TypeId,
                TimeStamp = x.Timestamp
            }).ToList();
        }
    }
}
