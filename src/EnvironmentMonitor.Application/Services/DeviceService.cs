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
using EnvironmentMonitor.Domain;

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
        private readonly IImageService _imageService;

        public DeviceService(IHubMessageService messageService, ILogger<DeviceService> logger, IUserService userService,
            IDeviceRepository deviceRepository, IMapper mapper, IStorageClient storageClient, IDateService dateService, IImageService imageService)
        {
            _messageService = messageService;
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
            _storageClient = storageClient;
            _dateService = dateService;
            _imageService = imageService;
        }
        public async Task Reboot(Guid identifier)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            _logger.LogInformation($"Trying to reboot device with identifier '{identifier}'");
            await _messageService.SendMessageToDevice(device.DeviceIdentifier, "REBOOT");
            await AddEvent(device.Id, DeviceEventTypes.RebootCommand, "Rebooted by UI", true);
        }

        public async Task SetMotionControlStatus(Guid identifier, MotionControlStatus status)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            var message = $"MOTIONCONTROLSTATUS:{(int)status}";
            _logger.LogInformation($"Sending message: '{message}' to device: {device.Id}");
            await _messageService.SendMessageToDevice(device.DeviceIdentifier, message);
            await AddEvent(device.Id, DeviceEventTypes.SetMotionControlStatus, $"Motion control status set to: {(int)status} ({status.ToString()})", true);
        }

        public async Task SetMotionControlDelay(Guid identifier, long delayMs)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            var message = $"MOTIONCONTROLDELAY: {delayMs}";
            _logger.LogInformation($"Sending message: '{message}' to device: {device.Id}");
            await _messageService.SendMessageToDevice(device.DeviceIdentifier, message);
            await AddEvent(device.Id, DeviceEventTypes.SetMotionControlStatus, $"Motion control delay set to: {(int)delayMs} ms", true);
        }

        public async Task<List<DeviceDto>> GetDevices(bool onlyVisible)
        {
            var devices = await _deviceRepository.GetDevices(new GetDevicesModel() { Ids = _userService.IsAdmin ? null : _userService.GetDevices(), OnlyVisible = true });
            return _mapper.Map<List<DeviceDto>>(devices);
        }

        public async Task<List<SensorDto>> GetSensors(List<Guid> identifiers)
        {
            var sensors = await _deviceRepository.GetSensors(new GetSensorsModel()
            {
                DevicesModel = new GetDevicesModel()
                {
                    Identifiers = identifiers
                }
            });
            sensors = sensors.Where(s => _userService.HasAccessToSensor(s.Id, AccessLevels.Read));
            return _mapper.Map<List<SensorDto>>(sensors);
        }

        public async Task<List<SensorDto>> GetSensors(List<int> deviceIds)
        {
            var sensors = new List<SensorDto>();
            var res = await _deviceRepository.GetSensors(new GetSensorsModel()
            {
                DevicesModel = new GetDevicesModel()
                {
                    Ids = deviceIds.Where(d => _userService.HasAccessToDevice(d, AccessLevels.Read)).ToList()
                }
            });
            return _mapper.Map<List<SensorDto>>(res.ToList());
        }

        public async Task<DeviceDto> GetDevice(string deviceIdentifier, AccessLevels accessLevel)
        {
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { DeviceIdentifiers = [deviceIdentifier] })).FirstOrDefault();
            if (device == null || !_userService.HasAccessToDevice(device.Id, accessLevel))
            {
                throw new UnauthorizedAccessException();
            }
            var mapped = _mapper.Map<DeviceDto>(device);
            return mapped;
        }

        public async Task<DeviceDto> GetDevice(Guid identifier, AccessLevels accessLevel)
        {
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault();
            if (device == null || !_userService.HasAccessToDevice(device.Id, accessLevel))
            {
                throw new UnauthorizedAccessException();
            }
            var mapped = _mapper.Map<DeviceDto>(device);
            return mapped;
        }

        public async Task<SensorDto?> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel)
        {
            var sensors = await _deviceRepository.GetSensors(new GetSensorsModel()
            {

                DevicesModel = new GetDevicesModel()
                {
                    Ids = [deviceId]
                },
                SensorIds = [sensorIdInternal]
            });

            if (sensors.Count() > 1)
            {
                _logger.LogError("More than one sensor found");
                return null;
            }
            var sensor = sensors.FirstOrDefault();
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
            var devices = await _deviceRepository.GetDevices(new GetDevicesModel() { Ids = [deviceId], OnlyVisible = false });
            var device = devices.FirstOrDefault();
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: '{deviceId}' not found");
            }

            await _deviceRepository.AddEvent(deviceId, type, message, saveChanges, datetimeUtc);
        }

        public async Task<List<DeviceInfoDto>> GetDeviceInfos(bool onlyVisible, List<Guid>? identifiers, bool getAttachments = false)
        {
            _logger.LogInformation($"Fetching device infos. Identifiers: {string.Join(",", identifiers ?? [])}");
            var infos = new List<DeviceInfo>();
            if (identifiers?.Any() == true)
            {
                infos = (await _deviceRepository.GetDeviceInfo(new GetDevicesModel() { Identifiers = identifiers, OnlyVisible = onlyVisible, GetAttachments = getAttachments })).Where(d => _userService.HasAccessToDevice(d.Device.Id, AccessLevels.Read)).ToList();
            }
            else
            {
                infos = await _deviceRepository.GetDeviceInfo(new GetDevicesModel()
                {
                    Ids = _userService.IsAdmin ? null : _userService.GetDevices(),
                    OnlyVisible = onlyVisible,
                    GetAttachments = getAttachments
                });
            }
            var listToReturn = _mapper.Map<List<DeviceInfoDto>>(infos);
            foreach (var info in listToReturn)
            {
                foreach (var attachment in info.Attachments)
                {
                    var matchingAttachment = infos.FirstOrDefault(x => x.Device.Id == info.Device.Id)?.Device.Attachments.FirstOrDefault(a => a.Guid == attachment.Guid)?.Attachment;
                    if (matchingAttachment != null)
                    {
                        try
                        {
                            var blobInfo = await _storageClient.GetBlobInfo(matchingAttachment.Name);
                            attachment.SizeInBytes = blobInfo.SizeInBytes;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to fetch blob info for attachment: '{matchingAttachment.Name}'");
                        }
                    }

                }
            }
            return listToReturn;
        }

        public async Task<List<DeviceEventDto>> GetDeviceEvents(Guid identifier)
        {
            var device = await GetDevice(identifier, AccessLevels.Read) ?? throw new UnauthorizedAccessException();
            var events = await _deviceRepository.GetDeviceEvents(device.Id);
            return _mapper.Map<List<DeviceEventDto>>(events);
        }

        public async Task AddAttachment(Guid identifier, UploadAttachmentModel fileModel)
        {
            var device = await GetDevice(identifier, AccessLevels.Write);
            var extension = Path.GetExtension(fileModel.FileName);
            var fileNameToSave = $"{device.Identifier}_{Guid.NewGuid()}{extension}";
            var res = await _storageClient.Upload(new UploadAttachmentModel()
            {
                FileName = fileNameToSave,
                ContentType = fileModel.ContentType,
                Stream = await _imageService.CompressToSize(fileModel.Stream),
            });

            await _deviceRepository.AddAttachment(device.Id, new Attachment()
            {
                Name = fileNameToSave,
                OriginalName = fileModel.FileName,
                FullPath = res.ToString(),
                Path = res.ToString(),
                Extension = extension,
                Created = _dateService.CurrentTime(),
                ContentType = fileModel.ContentType,
            },
            true);
        }

        public async Task DeleteAttachment(Guid identifier, Guid attachmentIdentifier)
        {
            _logger.LogInformation($"Removing attachment with identifier: '{attachmentIdentifier}' for device with identifier: '{identifier}'");
            var device = await GetDevice(identifier, AccessLevels.Write);
            var attachment = await _deviceRepository.GetAttachment(device.Id, attachmentIdentifier);
            await _deviceRepository.DeleteAttachment(device.Id, attachmentIdentifier, true);
            await _storageClient.DeleteBlob(attachment.Name);
        }

        public async Task<AttachmentDownloadModel?> GetAttachment(Guid identifier, Guid attachmentIdentifier)
        {
            _logger.LogInformation($"Trying to get attachment with identifier '{attachmentIdentifier}' for device: '{identifier}'");
            var device = await GetDevice(identifier, AccessLevels.Write);
            var attachment = await _deviceRepository.GetAttachment(device.Id, attachmentIdentifier);
            return await _storageClient.GetImageAsync(attachment.Name);
        }
        public async Task<AttachmentDownloadModel?> GetDefaultImage(Guid identifier)
        {
            var device = await GetDevice(identifier, AccessLevels.Read);
            var deviceInfo = (await _deviceRepository.GetDeviceInfo(new GetDevicesModel() { Ids = [device.Id], GetAttachments = true, OnlyVisible = false })).FirstOrDefault();
            var attachment = deviceInfo?.Device.Attachments.FirstOrDefault(x => x.IsDefaultImage);
            if (attachment == null)
            {
                return null;
            }
            return await _storageClient.GetImageAsync(attachment.Attachment.Name);
        }

        public async Task SetDefaultImage(Guid identifier, Guid attachmentGuid)
        {
            _logger.LogInformation($"Setting default image for device '{identifier}'");
            var device = await GetDevice(identifier, AccessLevels.Write);
            await _deviceRepository.SetDefaultImage(device.Id, attachmentGuid);
        }

        public async Task SetStatus(SetDeviceStatusModel model)
        {
            _logger.LogInformation($"Trying to set status for device: {model.DeviceId}. Status: {model.Status}");
            if (!_userService.HasAccessToDevice(model.DeviceId, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            await _deviceRepository.SetStatus(model, true);
        }

        public async Task<DeviceStatusModel> GetDeviceStatus(GetDeviceStatusModel model)
        {
            if (!_userService.HasAccessToDevices(model.DeviceIds, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }

            var returnList = await _deviceRepository.GetDevicesStatus(model);
            return new DeviceStatusModel()
            {
                DeviceStatuses = _mapper.Map<List<DeviceStatusDto>>(returnList),

            };
        }

        public async Task<DeviceInfoDto> UpdateDevice(UpdateDeviceDto model)
        {
            if (model.Device.Id <= 0 && !_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            else if (!_userService.HasAccessToDevice(model.Device.Id, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Ids = [model.Device.Id] })).FirstOrDefault();
            var updateModel = _mapper.Map<Device>(model.Device);
            var info = await _deviceRepository.AddOrUpdate(updateModel, true);
            return _mapper.Map<DeviceInfoDto>(info);
        }
    }
}
