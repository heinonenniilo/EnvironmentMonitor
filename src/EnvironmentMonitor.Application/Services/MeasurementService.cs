using AutoMapper;
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
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public MeasurementService(
            IMeasurementRepository measurement,
            ILogger<MeasurementService> logger,
            IUserService userService,
            IMapper mapper)
        {
            _measurementRepository = measurement;
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task AddMeasurements(SaveMeasurementsDto measurent)
        {
            var device = await _measurementRepository.GetDeviceByIdentifier(measurent.DeviceId);
            if (device == null)
            {
                _logger.LogInformation($"Could not find device with device id '{measurent.DeviceId}'");
                return;
            }
            if (!_userService.HasAccessToDevice(device.Id, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException("No Access");
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

        public async Task<MeasurementsModel> GetMeasurements(GetMeasurementsModel model)
        {
            if (model.SensorIds.Any(s => !_userService.HasAccessToSensor(s, AccessLevels.Read)))
            {
                throw new UnauthorizedAccessException();
            }
            var rows = _mapper.Map<List<MeasurementDto>>(await _measurementRepository.GetMeasurements(model));
            return new MeasurementsModel()
            {
                Measurements = rows,
                MeasurementsInfo = GetMeasurementInfo(rows, model.SensorIds)
            };
        }

        public async Task<MeasurementsBySensorModel> GetMeasurementsBySensor(GetMeasurementsModel model)
        {
            var returnList = new List<MeasurementsBySensorDto>();
            _logger.LogInformation("Getting measurements by sensor");
            var accessibleSensorIds = model.SensorIds.Where(d => _userService.HasAccessToSensor(d, AccessLevels.Read)).ToList();

            var result = await _measurementRepository.GetMeasurements(new GetMeasurementsModel()
            {
                SensorIds = accessibleSensorIds,
                To = model.To,
                From = model.From,
                LatestOnly = model.LatestOnly,
            });

            var measurements = _mapper.Map<List<MeasurementDto>>(result);
            var info = GetMeasurementInfo(measurements, accessibleSensorIds);
            foreach (var sensorId in model.SensorIds)
            {
                var rowToAdd = new MeasurementsBySensorDto()
                {
                    SensorId = sensorId,
                    Measurements = measurements.Where(x => x.SensorId == sensorId).ToList(),
                    LatestValues = info.FirstOrDefault(d => d.SensorId == sensorId)?.LatestValues ?? [],
                    MaxValues = info.FirstOrDefault(d => d.SensorId == sensorId)?.MaxValues ?? [],
                    MinValues = info.FirstOrDefault(d => d.SensorId == sensorId)?.MinValues ?? []
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
