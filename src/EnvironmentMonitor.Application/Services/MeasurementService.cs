using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
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
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IDeviceService _deviceService;
        private readonly IDateService _dateService;
        private readonly ILocationRepository _locationRepository;
        private readonly IDeviceRepository _deviceRepository;
        

        public MeasurementService(
            IMeasurementRepository measurement,
            ILogger<MeasurementService> logger,
            IUserService userService,
            IMapper mapper,
            IDeviceService deviceService,
            IDateService dateService,
            ILocationRepository locationRepository,
            IDeviceRepository deviceRepository)
        {
            _measurementRepository = measurement;
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
            _deviceService = deviceService;
            _dateService = dateService;
            _locationRepository = locationRepository;
            _deviceRepository = deviceRepository;
        }

        public async Task AddMeasurements(SaveMeasurementsDto measurement)
        {
            var device = await _deviceService.GetDevice(measurement.DeviceId, AccessLevels.Write);
            if (device == null)
            {
                _logger.LogWarning($"Could not find device with device id '{measurement.DeviceId}'");
                return;
            }
            if (!_userService.HasAccessToDevice(device.Id, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException("No Access");
            }
            _logger.LogInformation($"Found device with ID: {device.Id} for device identifier '{measurement.DeviceId}'");
            var measurementsToAdd = new List<Measurement>();
            DeviceMessage? deviceMessage = null;
            if (measurement.EnqueuedUtc != null)
            {
                deviceMessage = new DeviceMessage()
                {
                    TimeStamp = _dateService.UtcToLocal(measurement.EnqueuedUtc.Value),
                    TimeStampUtc = measurement.EnqueuedUtc.Value,
                    DeviceId = device.Id,
                    SequenceNumber = measurement.SequenceNumber,
                    FirstMessage = measurement.FirstMessage,
                    Created = _dateService.CurrentTime(),
                    Uptime = measurement.Uptime,
                    MessageCount = measurement.MessageCount,
                    Identifier = measurement.Identifier,
                    LoopCount = measurement.LoopCount,
                };
            }
            var isDuplicate = false;
            if (!string.IsNullOrEmpty(measurement.Identifier) && deviceMessage != null)
            {
                var existingMessage = await _measurementRepository.GetDeviceMessage(measurement.Identifier, device.Id);
                isDuplicate = existingMessage != null;
                deviceMessage.IsDuplicate = isDuplicate;
                await _measurementRepository.AddDeviceMessage(deviceMessage, isDuplicate);
            }

            if (isDuplicate)
            {
                _logger.LogWarning($"Duplicate message. Identifier: '{measurement.Identifier}'. Returning.");
                return;
            }

            foreach (var row in measurement.Measurements)
            {
                var sensor = await _deviceService.GetSensor(device.Id, row.SensorId, AccessLevels.Write);
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

                var createdAt = _dateService.CurrentTime();
                measurementsToAdd.Add(new Measurement()
                {
                    SensorId = sensor.Id,
                    Value = row.SensorValue,
                    Timestamp = _dateService.UtcToLocal(row.TimestampUtc),
                    CreatedAt = createdAt,
                    CreatedAtUtc = _dateService.LocalToUtc(createdAt),
                    TimestampUtc = row.TimestampUtc,
                    TypeId = row.TypeId
                });
            }

            _logger.LogInformation($"Adding {measurementsToAdd.Count} measurements for Device ({device.Id}): '{device.Name}'");
            if (measurement.FirstMessage)
            {
                await _deviceService.AddEvent(device.Id, DeviceEventTypes.Online, "First message after boot", false, measurement.EnqueuedUtc);
            }
            if (measurement.EnqueuedUtc != null && (_dateService.LocalToUtc(_dateService.CurrentTime()) - measurement.EnqueuedUtc).Value.TotalMinutes < ApplicationConstants.DeviceWarningLimitInMinutes)
            {
                await _deviceRepository.SetStatus(new SetDeviceStatusModel()
                {
                    DeviceId = device.Id,
                    Status = true,
                    TimeStamp = _dateService.UtcToLocal(measurement.EnqueuedUtc.Value),
                    Message = $"Measurement count: {measurementsToAdd.Count}",
                    DeviceMessage = deviceMessage
                }, false);
            }
            await _measurementRepository.AddMeasurements(measurementsToAdd, true, deviceMessage);
            _logger.LogInformation("Measurementsadded");
        }

        public async Task<MeasurementsModel> GetMeasurements(GetMeasurementsModel model)
        {
            if (model.SensorIds.Any(s => !_userService.HasAccessToSensor(s, AccessLevels.Read)))
            {
                throw new UnauthorizedAccessException();
            }

            if (!_userService.IsAdmin && model.SensorIds.Count == 0)
            {
                throw new InvalidOperationException("SensorIds must be defined");
            }

            var measurementRows = await _measurementRepository.GetMeasurements(model);

            var rows = _mapper.Map<List<MeasurementDto>>(await _measurementRepository.GetMeasurements(model));
            return new MeasurementsModel()
            {
                Measurements = _mapper.Map<List<MeasurementDto>>(measurementRows),
                MeasurementsInfo = GetMeasurementInfo(measurementRows.ToList(), model.SensorIds)
            };
        }

        public async Task<MeasurementsByLocationModel> GetMeasurementsByLocation(GetMeasurementsModel model)
        {
            if (!_userService.HasAccessToLocations(model.SensorIds, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel() { Ids = model.SensorIds, IncludeLocationSensors = true} );
            var locationSensors = locations.Select(x => x.LocationSensors).ToList();
            if (locationSensors.Count == 0)
            {
                throw new InvalidOperationException();
            }
            var sensorIds = locationSensors.SelectMany(x => x).Select(d => d.SensorId).ToList();
            var res = await _measurementRepository.GetMeasurements(new GetMeasurementsModel()
            {
                From = model.From,
                To = model.To,
                SensorIds = locationSensors.SelectMany(x => x).Select(d => d.SensorId).ToList()
            });

            var modelToReturn = new MeasurementsByLocationModel();
            foreach (var location in locations)
            {
                var measurementsInLocation = new List<MeasurementsBySensorDto>();
                foreach (var sensor in location.LocationSensors)
                {
                    var measurementsToCheck = res.Where(x => x.SensorId == sensor.SensorId && x.TypeId == sensor.TypeId).ToList();
                    var measurementsBySensor = _mapper.Map<List<MeasurementBaseDto>>(measurementsToCheck);
                    var infoRow = GetMeasurementInfo(measurementsToCheck, [sensor.SensorId]).FirstOrDefault();
                    var bySensorRow = new MeasurementsBySensorDto()
                    {
                        SensorId = sensor.SensorId,
                        Measurements = measurementsBySensor,
                        LatestValues = infoRow?.LatestValues ?? [],
                        MaxValues = infoRow?.MaxValues ?? [],
                        MinValues = infoRow?.MinValues ?? []
                    };
                    measurementsInLocation.Add(bySensorRow);
                }
                modelToReturn.Measurements.Add(new MeasurementsByLocationDto()
                {
                    Id = location.Id,
                    Measurements = measurementsInLocation,
                    Sensors = _mapper.Map<List<SensorDto>>(location.LocationSensors)
                });
            }
            return modelToReturn;
        }

        public async Task<MeasurementsBySensorModel> GetMeasurementsBySensor(GetMeasurementsModel model)
        {
            var returnList = new List<MeasurementsBySensorDto>();
            _logger.LogInformation("Getting measurements by sensor");
            if (model.SensorIds.Any(s => !_userService.HasAccessToSensor(s, AccessLevels.Read)))
            {
                throw new UnauthorizedAccessException();
            }
            var accessibleSensorIds = model.SensorIds.Where(d => _userService.HasAccessToSensor(d, AccessLevels.Read)).ToList();
            if (!_userService.IsAdmin && !accessibleSensorIds.Any())
            {
                throw new InvalidOperationException("No accessible sensors");
            }

            var result = await _measurementRepository.GetMeasurements(new GetMeasurementsModel()
            {
                SensorIds = accessibleSensorIds,
                To = model.To,
                From = model.From,
                LatestOnly = model.LatestOnly,
            });
            var info = GetMeasurementInfo(result.ToList(), accessibleSensorIds);
            foreach (var sensorId in model.SensorIds)
            {
                var rowToAdd = new MeasurementsBySensorDto()
                {
                    SensorId = sensorId,
                    Measurements = _mapper.Map<List<Measurement>, List<MeasurementBaseDto>>(result.Where(x => x.SensorId == sensorId).ToList()),
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

        public async Task<MeasurementsBySensorModel> GetMeasurementsByPublicSensor(GetMeasurementsModel model)
        {

            if (((model.To ?? _dateService.CurrentTime()) - model.From).TotalDays > ApplicationConstants.PublicMeasurementMaxLimitInDays)
            {
                throw new InvalidOperationException($"Max query range is {ApplicationConstants.PublicMeasurementMaxLimitInDays} days");
            }
            var publicSensors = await _measurementRepository.GetPublicSensors();

            if (publicSensors.Count == 0)
            {
                return new MeasurementsBySensorModel();
            }

            var sensorIds = publicSensors.Select(ps => ps.SensorId).ToList();
            var sensorFilterDictionary = new Dictionary<int, List<MeasurementTypes>?>();

            foreach (var publicSensor in publicSensors)
            {
                sensorFilterDictionary.Add(publicSensor.SensorId, publicSensor.TypeId == null ? null : [(MeasurementTypes)publicSensor.TypeId]);
            }

            var res = await _measurementRepository.GetMeasurements(new GetMeasurementsModel()
            {
                From = model.From,
                To = model.To,
                SensorsByTypeFilter = sensorFilterDictionary,
                LatestOnly = model.LatestOnly
            });

            var info = GetMeasurementInfo(res.ToList(), sensorIds);

            var returnModel = new MeasurementsBySensorModel()
            {
                Sensors = _mapper.Map<List<SensorDto>>(publicSensors),
            };
            foreach (var publicSensor in publicSensors)
            {
                var sensorIdToCheck = publicSensor.SensorId;
                var rowToAdd = new MeasurementsBySensorDto()
                {
                    SensorId = publicSensor.Id,
                    Measurements = _mapper.Map<List<Measurement>, List<MeasurementBaseDto>>(res.Where(x => x.SensorId == sensorIdToCheck).ToList()) ,
                    LatestValues = info.FirstOrDefault(d => d.SensorId == sensorIdToCheck)?.LatestValues ?? [],
                    MaxValues = info.FirstOrDefault(d => d.SensorId == sensorIdToCheck)?.MaxValues ?? [],
                    MinValues = info.FirstOrDefault(d => d.SensorId == sensorIdToCheck)?.MinValues ?? []
                };
                returnModel.Measurements.Add(rowToAdd);
            }
            return returnModel;
        }

        private List<MeasurementsInfoDto> GetMeasurementInfo(ICollection<Measurement> measurements, List<int> sensorIds)
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
                        rowToAdd.MinValues[(int)type] =  _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderBy(x => x.Value).First());
                        rowToAdd.MaxValues[(int)type] = _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.Value).First());
                        rowToAdd.LatestValues[(int)type] = _mapper.Map < Measurement, MeasurementDto>( measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.Timestamp).First());
                    }
                }
                returnList.Add(rowToAdd);
            }
            return returnList;
        }
    }
}
