using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            var device = await _measurementRepository.GetDeviceByIdentifier(measurent.DeviceId);
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

        public async Task<MeasurementsModel> GetMeasurements(GetMeasurementsModel model)
        {
            var rows = (await _measurementRepository.GetMeasurements(model)).Select(x => new MeasurementDto()
            {
                SensorId = x.SensorId,
                SensorValue = x.Value,
                TypeId = x.TypeId,
                TimestampUtc = x.TimestampUtc,
                Timestamp = x.Timestamp,
            }).ToList();
            return new MeasurementsModel()
            {
                Measurements = rows,
                MeasurementsInfo = GetMeasurementInfo(rows, model.SensorIds)
            };
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

        public async Task<MeasurementsBySensorModel> GetMeasurementsBySensor(GetMeasurementsModel model)
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
                    Measurements = measurements.Measurements,
                    LatestValues = measurements.MeasurementsInfo?.FirstOrDefault().LatestValues ?? [],
                    MaxValues = measurements.MeasurementsInfo?.FirstOrDefault().MaxValues ?? [],
                    MinValues = measurements.MeasurementsInfo?.FirstOrDefault().MinValues ?? []
                };
                returnList.Add(rowToAdd);
            }
            return new MeasurementsBySensorModel()
            {
                Measurements = returnList
            };
        }

        private DateTime UtcToLocalTime(DateTime utcDateTime)
        {
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TargetTimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
        }

        private List<MeasurementsInfoDto> GetMeasurementInfo(List<MeasurementDto> measurements, List<int> sensorIds)
        {
            var returnList = new List<MeasurementsInfoDto>();
            foreach (var sensorId in sensorIds)
            {
                var measurementsToCheck = measurements.Where(x => x.SensorId == sensorId).ToList();
                if (!measurementsToCheck.Any())
                    continue;
                var rowToAdd = new MeasurementsInfoDto() { SensorId = sensorId };
                foreach (MeasurementTypes type in Enum.GetValues(typeof(MeasurementTypes)))
                {
                    if (measurementsToCheck.Any(x => x.TypeId == (int)type && x.SensorId == sensorId))
                    {
                        rowToAdd.MinValues[(int)type] = measurementsToCheck.Where(x => x.TypeId == (int)type).OrderBy(x => x.SensorValue).First();
                        rowToAdd.MaxValues[(int)type] = measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.SensorValue).First();
                        rowToAdd.LatestValues[(int)type] = measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.Timestamp).First();
                    }
                }
                returnList.Add(rowToAdd);
            }
            return returnList;
        }
    }
}
