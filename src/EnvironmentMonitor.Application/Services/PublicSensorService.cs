using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Application.Services
{
    public class PublicSensorService : IPublicSensorService
    {
        private readonly IPublicSensorRepository _publicSensorRepository;
        private readonly IMeasurementRepository _measurementRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IDateService _dateService;
        private readonly ILogger<PublicSensorService> _logger;
        private readonly IMeasurementAnalyzeService _measurementInfoService;

        public PublicSensorService(
            IPublicSensorRepository publicSensorRepository,
            IMeasurementRepository measurementRepository,
            ISensorRepository sensorRepository,
            IUserService userService,
            IMapper mapper,
            IDateService dateService,
            ILogger<PublicSensorService> logger,
            IMeasurementAnalyzeService measurementInfoService)
        {
            _publicSensorRepository = publicSensorRepository;
            _measurementRepository = measurementRepository;
            _sensorRepository = sensorRepository;
            _userService = userService;
            _mapper = mapper;
            _dateService = dateService;
            _logger = logger;
            _measurementInfoService = measurementInfoService;
        }

        public async Task<MeasurementsBySensorModel> GetMeasurementsByPublicSensor(GetMeasurementsModel model)
        {
            var daysDifference = ((model.To ?? _dateService.CurrentTime()) - model.From).TotalDays;
            var limitInDays = ApplicationConstants.PublicMeasurementMaxLimitInDays;

            if (_userService.Roles?.Any() == true)
            {
                limitInDays = ApplicationConstants.PublicMeasurementMaxLimitInDaysForRegistered;
            }

            List<string> rolesToCheck = ["User", "Admin", "Viewer"];

            var userRoles = _userService.Roles ?? [];

            if (userRoles.Any(userRole => rolesToCheck.Any(roleToCheck => userRole.Equals(roleToCheck, StringComparison.OrdinalIgnoreCase))))
            {
                limitInDays = ApplicationConstants.PublicMeasurementMaxLimitInDaysForUsers;
            }

            if (daysDifference > limitInDays)
            {
                throw new ArgumentException($"Max query range is {limitInDays} days");
            }

            var publicSensors = await _publicSensorRepository.GetPublicSensors(new GetPublicSensorsModel()
            {
                Identifiers = model.SensorIdentifiers.Count != 0 ? model.SensorIdentifiers : null,
                IsActive = true
            });

            if (publicSensors.Count == 0)
            {
                return new MeasurementsBySensorModel();
            }

            var sensorIds = publicSensors.Select(ps => ps.Sensor.Identifier).ToList();
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
                LatestOnly = model.LatestOnly,
                MeasurementTypes = model.MeasurementTypes
            });

            var info = _measurementInfoService.GetMeasurementInfo(res.ToList(), sensorIds);

            var returnModel = new MeasurementsBySensorModel()
            {
                Sensors = _mapper.Map<List<SensorDto>>(publicSensors),
            };
            foreach (var publicSensor in publicSensors)
            {
                var sensorIdToCheck = publicSensor.Sensor.Identifier;
                var rowToAdd = new MeasurementsBySensorDto()
                {
                    SensorIdentifier = publicSensor.Identifier,
                    Measurements = _mapper.Map<List<MeasurementExtended>, List<MeasurementBaseDto>>(res.Where(x => x.SensorIdentifier == sensorIdToCheck).ToList()),
                    LatestValues = info.FirstOrDefault(d => d.SensorIdentifier == sensorIdToCheck)?.LatestValues ?? [],
                    MaxValues = info.FirstOrDefault(d => d.SensorIdentifier == sensorIdToCheck)?.MaxValues ?? [],
                    MinValues = info.FirstOrDefault(d => d.SensorIdentifier == sensorIdToCheck)?.MinValues ?? []
                };
                returnModel.Measurements.Add(rowToAdd);
            }
            return returnModel;
        }

        public async Task<List<SensorDto>> GetPublicSensors(GetPublicSensorsModel model)
        {
            if (!_userService.IsAdmin)
            {
                model.IsActive = true;
            }

            var publicSensors = await _publicSensorRepository.GetPublicSensors(model);
            return GetPublicSensorDtos(publicSensors);
        }

        public async Task<List<SensorDto>> ManagePublicSensors(ManagePublicSensorsRequest request)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can manage public sensors");
            }

            foreach (var item in request.AddOrUpdate)
            {
                if (item.Identifier != null)
                {
                    var existing = await _publicSensorRepository.GetPublicSensor(item.Identifier.Value);
                    if (existing == null)
                    {
                        throw new InvalidOperationException($"Public sensor with identifier '{item.Identifier}' not found.");
                    }

                    var sensor = await _sensorRepository.GetSensor(item.SensorIdentifier);
                    if (sensor == null)
                    {
                        throw new InvalidOperationException($"Sensor with identifier '{item.SensorIdentifier}' not found.");
                    }

                    existing.Name = item.Name;
                    existing.SensorId = sensor.Id;
                    existing.TypeId = item.TypeId;
                    existing.Active = item.Active;
                    existing.Latitude = item.Latitude;
                    existing.Longitude = item.Longitude;
                    await _publicSensorRepository.UpdatePublicSensor(existing, false);
                }
                else
                {
                    var sensor = await _sensorRepository.GetSensor(item.SensorIdentifier);
                    if (sensor == null)
                    {
                        throw new InvalidOperationException($"Sensor with identifier '{item.SensorIdentifier}' not found.");
                    }

                    var now = _dateService.CurrentTime();
                    var publicSensor = new PublicSensor()
                    {
                        Name = item.Name,
                        SensorId = sensor.Id,
                        Sensor = sensor,
                        TypeId = item.TypeId,
                        Active = item.Active,
                        Latitude = item.Latitude,
                        Longitude = item.Longitude,
                        Created = now,
                        CreatedUtc = _dateService.LocalToUtc(now),
                    };
                    await _publicSensorRepository.AddPublicSensor(publicSensor, false);
                }
            }

            foreach (var identifier in request.Remove)
            {
                var existing = await _publicSensorRepository.GetPublicSensor(identifier);
                if (existing == null)
                {
                    throw new InvalidOperationException($"Public sensor with identifier '{identifier}' not found.");
                }
                await _publicSensorRepository.DeletePublicSensor(existing, false);
            }

            await _publicSensorRepository.SaveChanges();

            var publicSensors = await _publicSensorRepository.GetPublicSensors(new GetPublicSensorsModel());
            return GetPublicSensorDtos(publicSensors);
        }

        // For admins, fill actual sensor identifier.
        private List<SensorDto> GetPublicSensorDtos(List<PublicSensor> publicSensors)
        {
            var returnList = _mapper.Map<List<SensorDto>>(publicSensors);
            if (_userService.IsAdmin)
            {
                foreach (var sensor in returnList)
                {
                    var matchingSensor = publicSensors.FirstOrDefault(ps => ps.Identifier == sensor.Identifier);
                    if (matchingSensor != null)
                    {
                        sensor.ParentIdentifier = matchingSensor.Sensor.Identifier;
                    }
                }
            }
            return returnList;
        }
    }
}
