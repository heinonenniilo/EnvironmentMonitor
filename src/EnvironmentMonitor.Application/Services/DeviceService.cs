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
        private readonly IMeasurementRepository _measurementRepository;
        private readonly IMapper _mapper;

        public DeviceService(IHubMessageService messageService, ILogger<DeviceService> logger, IUserService userService, 
            IMeasurementRepository measurementRepository, IMapper mapper)
        {
            _messageService = messageService;
            _logger = logger;
            _userService = userService;
            _measurementRepository = measurementRepository;
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
            var devices = await _measurementRepository.GetDevices(_userService.IsAdmin ? null : _userService.GetDevices());
            return devices.Select(x => new DeviceDto()
            {
                Id = x.Id,
                Name = x.Name,
                DeviceIdentifier = x.DeviceIdentifier,
            }).ToList();
        }

        public async Task<List<SensorDto>> GetSensors(List<string> DeviceIdentifier)
        {
            var sensors = await _measurementRepository.GetSensorsByDeviceIdentifiers(DeviceIdentifier);
            sensors = sensors.Where(s => _userService.HasAccessToSensor(s.Id, AccessLevels.Read));
            return _mapper.Map<List<SensorDto>>(sensors);
        }

        public async Task<List<SensorDto>> GetSensors(List<int> DeviceIds)
        {
            var sensors = new List<SensorDto>();
            foreach (var deviceId in DeviceIds.Where(d => _userService.HasAccessToDevice(d, AccessLevels.Read)))
            {
                var res = await _measurementRepository.GetSensorsByDeviceIdAsync(deviceId);
                sensors.AddRange(_mapper.Map<List<SensorDto>>(res));
            }
            return sensors;
        }

        public async Task<DeviceDto> GetDevice(string deviceIdentifier)
        {
            var device = await _measurementRepository.GetDeviceByIdentifier(deviceIdentifier);
            if (device == null || !_userService.HasAccessToDevice(device.Id, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }
            var mapped = _mapper.Map<DeviceDto>(device);
            return mapped;
        }
    }
}
