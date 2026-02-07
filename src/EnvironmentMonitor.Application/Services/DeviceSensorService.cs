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

            var existingSensor = await _sensorRepository.GetSensor(model.Identifier.Value)
                ?? throw new EntityNotFoundException($"Sensor with identifier: {model.Identifier} not found.");

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [model.DeviceIdentifier], OnlyVisible = false })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{model.DeviceIdentifier}' not found.");

            if (existingSensor.DeviceId != device.Id)
            {
                throw new InvalidOperationException("Sensor does not belong to the specified device.");
            }

            _logger.LogInformation($"Updating sensor: {model.Identifier}");

            existingSensor.Name = model.Name;
            existingSensor.ScaleMin = model.ScaleMin;
            existingSensor.ScaleMax = model.ScaleMax;
            existingSensor.Active = model.Active;
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

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [deviceIdentifier], OnlyVisible = false })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");

            var sensor = await _sensorRepository.GetSensor(sensorIdentifier)
                ?? throw new EntityNotFoundException($"Sensor with identifier: {sensorIdentifier} not found.");

            if (sensor.DeviceId != device.Id)
            {
                throw new InvalidOperationException("Sensor does not belong to the specified device.");
            }

            _logger.LogInformation($"Deleting sensor: {sensorIdentifier} from device: {deviceIdentifier}");

            await _sensorRepository.DeleteSensor(sensorIdentifier, true);

            _logger.LogInformation($"Successfully deleted sensor: {sensorIdentifier}");
        }
    }
}
