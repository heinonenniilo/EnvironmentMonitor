﻿using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace EnvironmentMonitor.Application.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly IMeasurementRepository _measurementRepository;
        private readonly ILogger<MeasurementService> _logger;
        private const string TargetTimeZone = "FLE Standard Time";

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
                    Timestamp = UtcToLocalTime(row.TimestampUtc),
                    CreatedAt = UtcToLocalTime(DateTime.UtcNow),
                    CreatedAtUtc = DateTime.UtcNow,
                    TimestampUtc = row.TimestampUtc,
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

        public async Task<List<DeviceDto>> GetDevices()
        {
            var devices = await _measurementRepository.GetDevices();
            return devices.Select(x => new DeviceDto()
            {
                Id = x.Id,
                Name = x.Name,
                DeviceIdentifier = x.DeviceIdentifier,
            }).ToList();
        }

        public async Task<List<MeasurementDto>> GetMeasurements(GetMeasurementsModel model)
        {
            var rows = await _measurementRepository.GetMeasurements(model);
            return rows.Select(x => new MeasurementDto()
            {
                SensorId = x.SensorId,
                SensorValue = x.Value,
                TypeId = x.TypeId,
                TimestampUtc = x.TimestampUtc,
                Timestamp = x.Timestamp,
            }).ToList();
        }

        public async Task<List<SensorDto>> GetSensors(List<string> DeviceIdentifier)
        {
            var sensors = await _measurementRepository.GetSensorsByDeviceIdentifiers(DeviceIdentifier);
            return sensors.Select(x => new SensorDto()
            {
                Id = x.Id,
                DeviceId = x.DeviceId,
                Name = x.Name,
                SensorId = x.SensorId,
                ScaleMax = x.ScaleMax,
                ScaleMin = x.ScaleMin,
            }).ToList();
        }

        private DateTime UtcToLocalTime(DateTime utcDateTime)
        {
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TargetTimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
        }

        public async Task<MeasurementsViewModel> GetMeasurementsBySensor(GetMeasurementsModel model)
        {
            var returnList = new List<MeasurementsBySensorDto>();
            _logger.LogInformation("Getting measurements by sensor");
            foreach (var sensorId in model.SensorIds)
            {
                var measurements = await GetMeasurements(new GetMeasurementsModel()
                {
                    SensorIds = [sensorId],
                    To = model.To,
                    From = model.From,
                });
                var rowToAdd = new MeasurementsBySensorDto()
                {
                    SensorId = sensorId,
                    Measurements = measurements
                };
                if (measurements.Any(x => x.TypeId == (int)MeasurementTypes.Humidity))
                {
                    rowToAdd.MinValues[MeasurementTypes.Humidity] = measurements.Where(x => x.TypeId == (int)MeasurementTypes.Humidity).OrderBy(x => x.SensorValue).First();
                    rowToAdd.MaxValues[MeasurementTypes.Humidity] = measurements.Where(x => x.TypeId == (int)MeasurementTypes.Humidity).OrderByDescending(x => x.SensorValue).First();
                }
                if (measurements.Any(x => x.TypeId == (int)MeasurementTypes.Temperature))
                {
                    rowToAdd.MinValues[MeasurementTypes.Temperature] = measurements.Where(x => x.TypeId == (int)MeasurementTypes.Temperature).OrderBy(x => x.SensorValue).First();
                    rowToAdd.MaxValues[MeasurementTypes.Temperature] = measurements.Where(x => x.TypeId == (int)MeasurementTypes.Temperature).OrderByDescending(x => x.SensorValue).First();
                }
                returnList.Add(rowToAdd);
            }
            return new MeasurementsViewModel()
            {
                Measurements = returnList
            };
        }
    }
}
