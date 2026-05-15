using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Application.Services
{
    public class DeviceSensorService : IDeviceSensorService
    {
        private readonly ILogger<DeviceSensorService> _logger;
        private readonly IUserService _userService;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly IMapper _mapper;

        public DeviceSensorService(
            ILogger<DeviceSensorService> logger,
            IUserService userService,
            IDeviceRepository deviceRepository,
            ISensorRepository sensorRepository,
            IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _sensorRepository = sensorRepository;
            _mapper = mapper;
        }

        public async Task<List<SensorInfoDto>> GetSensors(Guid deviceIdentifier)
        {
            if (!_userService.HasAccessToDevice(deviceIdentifier, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [deviceIdentifier] })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");

            var sensors = await _sensorRepository.GetSensorsByDevice(device.Id);
            return _mapper.Map<List<SensorInfoDto>>(sensors);
        }

        public async Task<SensorDto?> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel, bool? active = true)
        {
            var sensors = await _deviceRepository.GetSensors(new GetSensorsModel()
            {
                DevicesModel = new GetDevicesModel()
                {
                    Ids = [deviceId]
                },
                SensorIds = [sensorIdInternal],
                IsActive = active
            });

            if (sensors.Count() > 1)
            {
                _logger.LogError("More than one sensor found");
                return null;
            }


            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Ids = [deviceId] })).FirstOrDefault();
            var sensor = sensors.FirstOrDefault();

            if (sensor == null || (!_userService.HasAccessToSensor(sensor.Identifier, accessLevel) && !_userService.HasAccessToDevice(device.Identifier, accessLevel)))
            {
                if (sensor == null)
                {
                    _logger.LogError($"Could not find sensor with internal id: {sensorIdInternal} / Device Id: {deviceId}");
                }
                else
                {
                    _logger.LogError($"Not authorized to access sensor with id {sensor.Id}");
                }
                return null;
            }
            return _mapper.Map<SensorDto>(sensor);
        }

        public async Task<SensorInfoDto> AddSensor(AddOrUpdateSensorDto model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [model.DeviceIdentifier], OnlyVisible = false })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{model.DeviceIdentifier}' not found.");

            if (model.SensorId == null)
            {
                throw new ArgumentException("SensorId is required when adding a sensor.");
            }

            _logger.LogInformation($"Adding sensor '{model.Name}' (SensorId: {model.SensorId}) to device: {model.DeviceIdentifier}");

            var sensor = new Sensor
            {
                DeviceId = device.Id,
                SensorId = model.SensorId.Value,
                Name = model.Name,
                ScaleMin = model.ScaleMin,
                ScaleMax = model.ScaleMax,
                Active = model.Active,
                IsVirtual = model.IsVirtual,
                AggregationType = model.AggregationType,
            };

            var addedSensor = await _sensorRepository.AddSensor(sensor, true);

            _logger.LogInformation($"Successfully added sensor with identifier: {addedSensor.Identifier} to device: {model.DeviceIdentifier}");

            return _mapper.Map<SensorInfoDto>(addedSensor);
        }

        public async Task<SensorInfoDto> UpdateSensor(AddOrUpdateSensorDto model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            if (model.Identifier == null)
            {
                throw new ArgumentException("Identifier is required when updating a sensor.");
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() 
            { 
                Identifiers = [model.DeviceIdentifier], 
                OnlyVisible = false, 
                GetSensors = true })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{model.DeviceIdentifier}' not found.");
         
            var existingSensor = device.Sensors.FirstOrDefault(s => s.Identifier == model.Identifier.Value)
                ?? throw new EntityNotFoundException($"Sensor with identifier: {model.Identifier} not found on device: {model.DeviceIdentifier}.");

            _logger.LogInformation($"Updating sensor: {model.Identifier}");

            existingSensor.Name = model.Name;
            existingSensor.ScaleMin = model.ScaleMin;
            existingSensor.ScaleMax = model.ScaleMax;
            existingSensor.Active = model.Active;
            existingSensor.IsVirtual = model.IsVirtual;
            existingSensor.AggregationType = model.AggregationType;

            if (model.SensorId != null)
            {
                existingSensor.SensorId = model.SensorId.Value;
            }

            var updatedSensor = await _sensorRepository.UpdateSensor(existingSensor, true);

            _logger.LogInformation($"Successfully updated sensor: {model.Identifier}");

            return _mapper.Map<SensorInfoDto>(updatedSensor);
        }

        public async Task DeleteSensor(Guid deviceIdentifier, Guid sensorIdentifier)
        {
            if (!_userService.HasAccessToDevice(deviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() 
            { 
                Identifiers = [deviceIdentifier], 
                GetSensors = true, 
                OnlyVisible = false 
            })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");

            var sensor = device.Sensors.FirstOrDefault(s => s.Identifier == sensorIdentifier)
                ?? throw new EntityNotFoundException($"Sensor with identifier: {sensorIdentifier} not found.");

            _logger.LogInformation($"Deleting sensor: {sensorIdentifier} from device: {deviceIdentifier}");

            await _sensorRepository.DeleteSensor(sensorIdentifier, true);

            _logger.LogInformation($"Successfully deleted sensor: {sensorIdentifier}");
        }

        public async Task<SensorInfoDto> UpdateVirtualSensorRows(UpdateVirtualSensorRowsDto model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [model.DeviceIdentifier],
                OnlyVisible = false,
                GetSensors = true
            })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{model.DeviceIdentifier}' not found.");

            var sensor = device.Sensors.FirstOrDefault(s => s.Identifier == model.SensorIdentifier)
                ?? throw new EntityNotFoundException($"Sensor with identifier: '{model.SensorIdentifier}' not found on device: '{model.DeviceIdentifier}'.");

            if (!sensor.IsVirtual)
            {
                throw new InvalidOperationException($"Sensor with identifier: '{model.SensorIdentifier}' is not a virtual sensor.");
            }

            foreach (var rowToDelete in model.RowsToDelete)
            {
                var valueSensor = await _sensorRepository.GetSensor(rowToDelete)
                    ?? throw new EntityNotFoundException($"Sensor with identifier: '{rowToDelete}' not found.");

                await _sensorRepository.DeleteVirtualSensorRow(sensor.Id, valueSensor.Id, false);
            }

            foreach (var rowToAdd in model.RowsToAdd)
            {
                var valueSensor = await _sensorRepository.GetSensor(rowToAdd.ValueSensorIdentifier)
                    ?? throw new EntityNotFoundException($"Sensor with identifier: '{rowToAdd.ValueSensorIdentifier}' not found.");

                await _sensorRepository.AddVirtualSensorRow(new Domain.Entities.VirtualSensorRow
                {
                    VirtualSensorId = sensor.Id,
                    ValueSensorId = valueSensor.Id,
                    TypeId = rowToAdd.TypeId
                }, false);
            }

            await _sensorRepository.SaveChanges();

            _logger.LogInformation($"Updated VirtualSensorRows for sensor: {model.SensorIdentifier}");

            var updatedSensor = await _sensorRepository.GetSensor(model.SensorIdentifier)
                ?? throw new EntityNotFoundException($"Sensor with identifier: '{model.SensorIdentifier}' not found after update.");

            return _mapper.Map<SensorInfoDto>(updatedSensor);
        }

        public async Task<List<SensorDto>> GetSensors(List<Guid> deviceIdentifiers)
        {
            if (deviceIdentifiers == null || deviceIdentifiers.Count == 0)
            {
                throw new ArgumentException("Identifiers list cannot be null or empty.");
            }

            if (!_userService.HasAccessToDevices(deviceIdentifiers, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }

            var sensors = await _deviceRepository.GetSensors(new GetSensorsModel()
            {
                DevicesModel = new GetDevicesModel()
                {
                    Identifiers = deviceIdentifiers
                }
            });

            return _mapper.Map<List<SensorDto>>(sensors);
        }
    }
}
