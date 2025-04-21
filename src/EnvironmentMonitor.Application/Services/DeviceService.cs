using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
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
        private readonly IStorageClient _storageClient;
        private readonly IMapper _mapper;
        private readonly IDateService _dateService;

        public DeviceService(IHubMessageService messageService, ILogger<DeviceService> logger, IUserService userService,
            IDeviceRepository deviceRepository, IMapper mapper, IStorageClient storageClient, IDateService dateService)
        {
            _messageService = messageService;
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
            _storageClient = storageClient;
            _dateService = dateService;
        }
        public async Task Reboot(string deviceIdentifier)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = await _deviceRepository.GetDeviceByIdentifier(deviceIdentifier) ?? throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");
            _logger.LogInformation($"Trying to reboot device with identifier '{deviceIdentifier}'");
            await _messageService.SendMessageToDevice(deviceIdentifier, "REBOOT");
            await AddEvent(device.Id, DeviceEventTypes.RebootCommand, "Rebooted by UI", true);
        }

        public async Task SetMotionControlStatus(string deviceIdentifier, MotionControlStatus status)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = await _deviceRepository.GetDeviceByIdentifier(deviceIdentifier);
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");
            }
            var message = $"MOTIONCONTROLSTATUS:{(int)status}";
            _logger.LogInformation($"Sending message: '{message}' to device: {device.Id}");
            await _messageService.SendMessageToDevice(deviceIdentifier, message);
            await AddEvent(device.Id, DeviceEventTypes.SetMotionControlStatus, $"Motion control status set to: {(int)status} ({status.ToString()})", true);
        }

        public async Task SetMotionControlDelay(string deviceIdentifier, long delayMs)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = await _deviceRepository.GetDeviceByIdentifier(deviceIdentifier);
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");
            }
            var message = $"MOTIONCONTROLDELAY:{delayMs}";
            _logger.LogInformation($"Sending message: '{message}' to device: {device.Id}");
            await _messageService.SendMessageToDevice(deviceIdentifier, message);
            await AddEvent(device.Id, DeviceEventTypes.SetMotionControlStatus, $"Motion control delay set to: {(int)delayMs} ms", true);
        }

        public async Task<List<DeviceDto>> GetDevices()
        {
            var devices = await _deviceRepository.GetDevices(_userService.IsAdmin ? null : _userService.GetDevices());
            return _mapper.Map<List<DeviceDto>>(devices);
        }

        public async Task<List<SensorDto>> GetSensors(List<string> deviceIdentifiers)
        {
            var sensors = await _deviceRepository.GetSensorsByDeviceIdentifiers(deviceIdentifiers);
            sensors = sensors.Where(s => _userService.HasAccessToSensor(s.Id, AccessLevels.Read));
            return _mapper.Map<List<SensorDto>>(sensors);
        }

        public async Task<List<SensorDto>> GetSensors(List<int> deviceIds)
        {
            var sensors = new List<SensorDto>();
            var res = await _deviceRepository.GetSensorsByDeviceIdsAsync(deviceIds.Where(d => _userService.HasAccessToDevice(d, AccessLevels.Read)).ToList());
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

        public async Task<SensorDto?> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel)
        {
            var sensor = await _deviceRepository.GetSensor(deviceId, sensorIdInternal);
            if (sensor == null || !_userService.HasAccessToSensor(sensor.Id, accessLevel))
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

        public async Task AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges, DateTime? datetimeUtc = null)
        {
            if (!_userService.HasAccessToDevice(deviceId, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            var devices = await _deviceRepository.GetDevices([deviceId], false);
            var device = devices.FirstOrDefault();
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: '{deviceId}' not found");
            }

            await _deviceRepository.AddEvent(deviceId, type, message, saveChanges, datetimeUtc);
        }

        public async Task<List<DeviceInfoDto>> GetDeviceInfos(bool onlyVisible, List<string>? identifiers)
        {
            _logger.LogInformation($"Fetching device infos. Identifiers: {string.Join(",", identifiers ?? [])}");
            var infos = new List<DeviceInfo>();
            if (identifiers?.Any() == true)
            {
                infos = (await _deviceRepository.GetDeviceInfo(identifiers, onlyVisible)).Where(d => _userService.HasAccessToDevice(d.Device.Id, AccessLevels.Read)).ToList();
            }
            else
            {
                infos = await _deviceRepository.GetDeviceInfo(_userService.IsAdmin ? null : _userService.GetDevices(), onlyVisible);
            }
            return _mapper.Map<List<DeviceInfoDto>>(infos);
        }

        public async Task<List<DeviceEventDto>> GetDeviceEvents(string identifier)
        {
            var device = await GetDevice(identifier, AccessLevels.Read) ?? throw new UnauthorizedAccessException();
            var events = await _deviceRepository.GetDeviceEvents(identifier);
            return _mapper.Map<List<DeviceEventDto>>(events);
        }

        public async Task SetDefaultImage(string deviceIdentifier, Stream fileStream, string fileName)
        {
            var device = await _deviceRepository.GetDeviceByIdentifier(deviceIdentifier);
            if (device == null || !_userService.HasAccessTo(EntityRoles.Device, device.Id, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            var fileNameToSave = $"{deviceIdentifier}_{Guid.NewGuid()}_{fileName}";
            var res = await _storageClient.Upload(fileStream, fileNameToSave);
            device.DefaultImage = new Attachment()
            {
                Name = fileNameToSave,
                FullPath = res.ToString(),
                Path = res.ToString(),
                Extension = $"{Path.GetExtension(fileName)}",
                CreatedAt = _dateService.CurrentTime()
            };
            await _deviceRepository.SaveChanges();
        }

        public async Task<AttachmentInfoModel?> GetDefaultImage(string deviceIdentifier)
        {
            var device = await _deviceRepository.GetDeviceByIdentifier(deviceIdentifier);
            if (device == null || !_userService.HasAccessTo(EntityRoles.Device, device.Id, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            if (device.DefaultImage == null)
            {
                return null;
            }
            return await _storageClient.GetImageAsync(device.DefaultImage.Name);
        }
    }
}
