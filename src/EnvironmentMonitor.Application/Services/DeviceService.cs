using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IHubMessageService _messageService;
        private readonly ILogger<DeviceService> _logger;
        private readonly IUserService _userService;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IMapper _mapper;

        public DeviceService(IHubMessageService messageService, ILogger<DeviceService> logger, IUserService userService, 
            IDeviceRepository deviceRepository, IMapper mapper)
        {
            _messageService = messageService;
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
        }
        public async Task Reboot(string deviceIdentifier)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            _logger.LogInformation($"Trying to reboot device with identifier '{deviceIdentifier}'");
            await _messageService.SendMessageToDevice(deviceIdentifier, "REBOOT");
        }

        public async Task<List<DeviceDto>> GetDevices()
        {
            var devices = await _deviceRepository.GetDevices(_userService.IsAdmin ? null : _userService.GetDevices());
            return _mapper.Map<List<DeviceDto>>(devices);
        }

        public async Task<List<SensorDto>> GetSensors(List<string> DeviceIdentifier)
        {
            var sensors = await _deviceRepository.GetSensorsByDeviceIdentifiers(DeviceIdentifier);
            sensors = sensors.Where(s => _userService.HasAccessToSensor(s.Id, AccessLevels.Read));
            return _mapper.Map<List<SensorDto>>(sensors);
        }

        public async Task<List<SensorDto>> GetSensors(List<int> DeviceIds)
        {
            var sensors = new List<SensorDto>();
            var res = await _deviceRepository.GetSensorsByDeviceIdsAsync(DeviceIds.Where(d => _userService.HasAccessToDevice(d, AccessLevels.Read)).ToList());
            return _mapper.Map<List<SensorDto>>(res);
        }

        public async Task<DeviceDto> GetDevice(string deviceIdentifier, AccessLevels accessLevel)
        {
            var device = await _deviceRepository.GetDeviceByIdentifier(deviceIdentifier);
            if (device == null || !_userService.HasAccessToDevice(device.Id, accessLevel))
            {
                throw new UnauthorizedAccessException();
            }
            var mapped = _mapper.Map<DeviceDto>(device);
            return mapped;
        }

        public async Task<SensorDto> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel)
        {
            var sensor = await _deviceRepository.GetSensor(deviceId, sensorIdInternal);

            if (sensor == null || !_userService.HasAccessToSensor(sensor.Id, accessLevel))
            {
                throw new UnauthorizedAccessException();
            }
            return _mapper.Map<SensorDto>(sensor);
        }
    }
}
