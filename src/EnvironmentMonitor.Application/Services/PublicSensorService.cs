using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
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

        public PublicSensorService(
            IPublicSensorRepository publicSensorRepository,
            IMeasurementRepository measurementRepository,
            ISensorRepository sensorRepository,
            IUserService userService,
            IMapper mapper,
            IDateService dateService,
            ILogger<PublicSensorService> logger)
        {
            _publicSensorRepository = publicSensorRepository;
            _measurementRepository = measurementRepository;
            _sensorRepository = sensorRepository;
            _userService = userService;
            _mapper = mapper;
            _dateService = dateService;
            _logger = logger;
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
                throw new InvalidOperationException($"Max query range is {limitInDays} days");
            }

            var publicSensors = await _publicSensorRepository.GetPublicSensors(model.SensorIdentifiers.Count != 0 ? model.SensorIdentifiers : null);

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

            var info = GetMeasurementInfo(res.ToList(), sensorIds);

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

        public async Task<List<SensorDto>> GetPublicSensors()
        {
            var publicSensors = await _publicSensorRepository.GetPublicSensors();
            var mapped = _mapper.Map<List<SensorDto>>(publicSensors);

            if (_userService.IsAdmin)
            {
                foreach (var dto in mapped)
                {
                    var publicSensor = publicSensors.FirstOrDefault(ps => ps.Identifier == dto.Identifier);
                    if (publicSensor?.Sensor != null)
                    {
                        dto.ParentIdentifier = publicSensor.Sensor.Identifier;
                    }
                }
            }

            return mapped;
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

            var publicSensors = await _publicSensorRepository.GetPublicSensors();
            return _mapper.Map<List<SensorDto>>(publicSensors);
        }

        private List<MeasurementsInfoDto> GetMeasurementInfo(ICollection<MeasurementExtended> measurements, List<Guid> sensorIds)
        {
            var returnList = new List<MeasurementsInfoDto>();
            foreach (var sensorId in sensorIds)
            {
                var measurementsToCheck = measurements.Where(x => x.SensorIdentifier == sensorId).ToList();
                if (!measurementsToCheck.Any())
                    continue;
                var rowToAdd = new MeasurementsInfoDto() { SensorIdentifier = sensorId };
                foreach (MeasurementTypes type in Enum.GetValues(typeof(MeasurementTypes)))
                {
                    if (measurementsToCheck.Any(x => x.TypeId == (int)type && x.SensorIdentifier == sensorId))
                    {
                        rowToAdd.MinValues[(int)type] = _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderBy(x => x.Value).First());
                        rowToAdd.MaxValues[(int)type] = _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.Value).First());
                        rowToAdd.LatestValues[(int)type] = _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.Timestamp).First());
                    }
                }
                returnList.Add(rowToAdd);
            }
            return returnList;
        }
    }
}
