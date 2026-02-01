using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.AddModels;
using EnvironmentMonitor.Domain.Models.GetModels;
using EnvironmentMonitor.Domain.Models.Pagination;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly ILogger<DeviceService> _logger;
        private readonly IUserService _userService;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDeviceEmailService _deviceEmailService;
        private readonly IStorageClient _storageClient;
        private readonly IMapper _mapper;
        private readonly IDateService _dateService;
        private readonly IImageService _imageService;
        private readonly IKeyVaultClient _keyVaultClient;
        private readonly DeviceSettings _deviceSettings;

        public DeviceService(ILogger<DeviceService> logger, IUserService userService,
            IDeviceRepository deviceRepository, IDeviceEmailService deviceEmailService, IMapper mapper, IStorageClient storageClient, IDateService dateService,
            IImageService imageService, IKeyVaultClient keyVaultClient, DeviceSettings deviceSettings)
        {
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _deviceEmailService = deviceEmailService;
            _mapper = mapper;
            _storageClient = storageClient;
            _dateService = dateService;
            _imageService = imageService;
            _keyVaultClient = keyVaultClient;
            _deviceSettings = deviceSettings;
        }

        public async Task<List<DeviceDto>> GetDevices(bool onlyVisible, bool getLocation)
        {
            var devices = await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = _userService.IsAdmin ? null : _userService.GetDevices(),
                OnlyVisible = onlyVisible,
                GetLocation = getLocation
            });
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
            sensors = sensors.Where(s => _userService.HasAccessToSensor(s.Identifier, AccessLevels.Read)).ToList();
            return _mapper.Map<List<SensorDto>>(sensors);
        }

        public async Task<List<SensorDto>> GetSensors(List<int> deviceIds)
        {
            var sensors = new List<SensorDto>();
            var devices = await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Ids = deviceIds
            });

            if (devices.Count == 0 || devices.Any(d => !_userService.HasAccessToDevice(d.Identifier, AccessLevels.Read)))
            {
                _logger.LogWarning($"Unauthorized access to devices: {string.Join(", ", devices.Select(x => x.Id))}");
                throw new UnauthorizedAccessException();
            }

            var res = await _deviceRepository.GetSensors(new GetSensorsModel()
            {
                DevicesModel = new GetDevicesModel()
                {
                    Ids = devices.Select(x => x.Id).ToList()
                }
            });
            return _mapper.Map<List<SensorDto>>(res.ToList());
        }

        public async Task<DeviceDto> GetDevice(string deviceIdentifier, AccessLevels accessLevel)
        {
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { DeviceIdentifiers = [deviceIdentifier] })).FirstOrDefault();
            if (device == null || !_userService.HasAccessToDevice(device.Identifier, accessLevel))
            {
                throw new UnauthorizedAccessException();
            }
            var mapped = _mapper.Map<DeviceDto>(device);
            return mapped;
        }

        public async Task<DeviceDto> GetDevice(Guid identifier, AccessLevels accessLevel)
        {
            if (!_userService.HasAccessToDevice(identifier, accessLevel))
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault();

            if (device == null)
            {
                throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
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

        public async Task AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges, DateTime? datetimeUtc = null)
        {

            var devices = await _deviceRepository.GetDevices(new GetDevicesModel() { Ids = [deviceId], OnlyVisible = false });
            var device = devices.FirstOrDefault();
            if (device == null || !_userService.HasAccessToDevice(device.Identifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: '{deviceId}' not found");
            }

            await _deviceRepository.AddEvent(deviceId, type, message, saveChanges, datetimeUtc);
        }

        public async Task<List<DeviceInfoDto>> GetDeviceInfos(bool onlyVisible, List<Guid>? identifiers, bool getAttachments = false, bool getLocation = false, bool getAttributes = false, 
            bool getContacts = false, bool? isVirtual = null)
        {
            _logger.LogInformation($"Fetching device infos. Identifiers: {string.Join(",", identifiers ?? [])}");
            var infos = new List<DeviceInfo>();
            if (identifiers?.Any() == true)
            {
                infos = (await _deviceRepository.GetDeviceInfo(new GetDevicesModel()
                {
                    Identifiers = identifiers,
                    OnlyVisible = onlyVisible,
                    GetAttachments = getAttachments,
                    GetLocation = getLocation,
                    GetAttributes = getAttributes,
                    GetContacts = getContacts,
                    IsVirtual = isVirtual
                }))
                .Where(d => _userService.HasAccessToDevice(d.Device.Identifier, AccessLevels.Read)).ToList();
            }
            else
            {
                infos = await _deviceRepository.GetDeviceInfo(new GetDevicesModel()
                {
                    Identifiers = _userService.IsAdmin ? null : _userService.GetDevices(),
                    OnlyVisible = onlyVisible,
                    GetAttachments = getAttachments,
                    GetLocation = getLocation,
                    GetAttributes = getAttributes,
                    IsVirtual = isVirtual, 
                });
            }
            var listToReturn = _mapper.Map<List<DeviceInfoDto>>(infos);
            foreach (var info in listToReturn)
            {
                foreach (var attachment in info.Attachments)
                {
                    var matchingAttachment = infos.FirstOrDefault(x => x.Device.Identifier == info.Device.Identifier)?.Device.Attachments.FirstOrDefault(a => a.Guid == attachment.Guid)?.Attachment;
                    if (matchingAttachment != null)
                    {
                        if (matchingAttachment.IsSecret)
                        {
                            continue;
                        }
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
            if (!_userService.HasAccessToDevice(identifier, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            var events = await _deviceRepository.GetDeviceEvents(device.Id);
            return _mapper.Map<List<DeviceEventDto>>(events);
        }

        public async Task AddAttachment(UploadDeviceAttachmentModel fileModel)
        {
            if (!_userService.HasAccessToDevice(fileModel.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [fileModel.DeviceIdentifier] })).FirstOrDefault();
            var extension = Path.GetExtension(fileModel.FileName);
            var fileNameToSave = fileModel.IsSecret ? $"{Guid.NewGuid()}" : $"{device.Identifier}_{Guid.NewGuid()}{extension}";

            string fullPath = string.Empty;
            string path = string.Empty;
            if (fileModel.IsSecret)
            {
                var secretName = await _keyVaultClient.StoreStreamAsSecretAsync(fileNameToSave, fileModel.Stream);
                fullPath = secretName.Identifier.ToString();
                path = secretName.SecretName;
            }
            else
            {
                var res = await _storageClient.Upload(new UploadAttachmentModel()
                {
                    FileName = fileNameToSave,
                    ContentType = fileModel.ContentType,
                    Stream = fileModel.IsDeviceImage ? await _imageService.CompressToSize(fileModel.Stream) : fileModel.Stream
                });
                fullPath = res.ToString();
                path = res.ToString();
            }

            await _deviceRepository.AddAttachment(new AddDeviceAttachmentModel()
            {
                Attachment = new Attachment()
                {
                    Name = fileNameToSave,
                    OriginalName = fileModel.FileName,
                    FullPath = fullPath,
                    Path = path,
                    Extension = extension,
                    Created = _dateService.CurrentTime(),
                    ContentType = fileModel.ContentType,
                    IsSecret = fileModel.IsSecret,
                },
                IsDeviceImage = fileModel.IsDeviceImage,
                Identifier = device.Identifier,
                SaveChanges = true,
            });
        }

        public async Task DeleteAttachment(Guid identifier, Guid attachmentIdentifier)
        {
            if (!_userService.HasAccessToDevice(identifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            _logger.LogInformation($"Removing attachment with identifier: '{attachmentIdentifier}' for device with identifier: '{identifier}'");
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            var attachment = await _deviceRepository.GetAttachment(device.Id, attachmentIdentifier);
            await _deviceRepository.DeleteAttachment(device.Id, attachmentIdentifier, true);
            if (attachment.IsSecret)
            {
                await _keyVaultClient.DeleteSecretAsync(attachment.Name);
            }
            else
            {
                await _storageClient.DeleteBlob(attachment.Name);
            }
        }

        public async Task<AttachmentDownloadModel?> GetAttachment(Guid identifier, Guid attachmentIdentifier)
        {
            if (!_userService.HasAccessToDevice(identifier, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }
            _logger.LogInformation($"Trying to get attachment with identifier '{attachmentIdentifier}' for device: '{identifier}'");
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            var attachment = await _deviceRepository.GetAttachment(device.Id, attachmentIdentifier);

            if (attachment.IsSecret)
            {
                return await _keyVaultClient.GetSecretAsStreamAsync(attachment.Name);
            }
            else
            {
                return await _storageClient.GetImageAsync(attachment.Name);
            }
        }

        public async Task<AttachmentDownloadModel?> GetDefaultImage(Guid identifier)
        {
            if (!_userService.HasAccessToDevice(identifier, AccessLevels.Read))
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault();
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            }
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
            if (!_userService.HasAccessToDevice(identifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault();

            if (device == null)
            {
                throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            }
            await _deviceRepository.SetDefaultImage(device.Id, attachmentGuid);
        }

        public async Task SetStatus(SetDeviceStatusModel model, bool saveChanges)
        {
            _logger.LogInformation($"Trying to set status for device: {model.Idenfifier}. Status: {model.Status}");
            if (!_userService.HasAccessToDevice(model.Idenfifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [model.Idenfifier],
                OnlyVisible = false
            }))
            .FirstOrDefault();

            var currentStatus = (await _deviceRepository.GetDevicesStatus(new GetDeviceStatusModel()
            {
                DeviceIdentifiers = [model.Idenfifier],
                LatestOnly = true
            })).FirstOrDefault();

            if (device == null || !_userService.HasAccessToDevice(device.Identifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var updatedStatus = await _deviceRepository.SetStatus(model, false);
            if (updatedStatus == null && saveChanges)
            {
                await _deviceRepository.SaveChanges();
                _logger.LogInformation($"Device status not changed for device: {model.Idenfifier}");
                return;
            }

            if (updatedStatus != null)
            {
                if (_deviceSettings.SendDeviceEmails)
                {
                    _logger.LogInformation($"Queuing device email for device {device.Name} ({device.Id}). Type: {currentStatus?.Status}");
                    await _deviceEmailService.QueueDeviceStatusEmail(model, updatedStatus, currentStatus, saveChanges);
                }
                else
                {
                    _logger.LogWarning($"Device emails are disabled. Not sending email for device {device.Name} ({device.Id}). Type: {currentStatus?.Status}");
                }
            }
        }

        public async Task<DeviceStatusModel> GetDeviceStatus(GetDeviceStatusModel model)
        {
            if (model.DeviceIdentifiers.Count == 0)
            {
                throw new ArgumentException("No device identifiers provided");
            }
            if (!_userService.HasAccessToDevices(model.DeviceIdentifiers, AccessLevels.Read))
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
            if (!_userService.HasAccessToDevice(model.Device.Identifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [model.Device.Identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{model.Device.Identifier}' not found.");
            var updateModel = _mapper.Map<Device>(model.Device);
            updateModel.Id = device.Id;
            updateModel.CommunicationChannelId = model.CommunicationChannelId;
            var info = await _deviceRepository.AddOrUpdate(updateModel, true);
            return _mapper.Map<DeviceInfoDto>(info);
        }

        public async Task<PaginatedResult<DeviceMessageDto>> GetDeviceMessages(GetDeviceMessagesModel model)
        {
            List<Guid>? deviceIdFilter = null;
            if (model.DeviceIdentifiers?.Any() == true)
            {
                if (!_userService.HasAccessToDevices(model.DeviceIdentifiers, AccessLevels.Read))
                {
                    _logger.LogWarning($"No access to devices: {model.DeviceIdentifiers}");
                    throw new UnauthorizedAccessException();
                }
                deviceIdFilter = model.DeviceIdentifiers;
            }
            if (deviceIdFilter == null && !_userService.IsAdmin)
            {
                var deviceIds = _userService.GetDevices();
                _logger.LogInformation($"User has access to: {deviceIds} devices");
                if (deviceIds.Count == 0)
                {
                    throw new UnauthorizedAccessException();
                }
                deviceIdFilter = deviceIds;
            }
            if (model.LocationIdentifiers?.Any() == true)
            {
                if (!_userService.HasAccessToLocations(model.LocationIdentifiers, AccessLevels.Read))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            model.DeviceIdentifiers = deviceIdFilter;
            var res = await _deviceRepository.GetDeviceMessages(model);
            return new PaginatedResult<DeviceMessageDto>()
            {
                Items = _mapper.Map<List<DeviceMessageDto>>(res.Items),
                PageNumber = res.PageNumber,
                PageSize = res.PageSize,
                TotalCount = res.TotalCount,
            };
        }

        public async Task<DeviceContactDto> AddDeviceContact(AddOrUpdateDeviceContactDto model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation($"Adding device contact for device: {model.DeviceIdentifier}");

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [model.DeviceIdentifier],
                OnlyVisible = false
            })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{model.DeviceIdentifier}' not found.");
            var contact = await _deviceRepository.AddDeviceContact(device.Id, model.Email, true);

            _logger.LogInformation($"Successfully added device contact with identifier: {contact.Identifier} for device: {model.DeviceIdentifier}");

            return _mapper.Map<DeviceContactDto>(contact);
        }

        public async Task<DeviceContactDto> UpdateDeviceContact(AddOrUpdateDeviceContactDto model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            if (model.Identifier == null)
            {
                throw new ArgumentException("Identifier is required when updating a device contact.");
            }

            var existingContact = await _deviceRepository.GetDeviceContact(model.Identifier.Value);

            if (existingContact == null)
            {
                throw new EntityNotFoundException($"Device contact with identifier: {model.Identifier} not found.");
            }

            _logger.LogInformation($"Updating device contact: {model.Identifier}");

            var updatedContact = await _deviceRepository.UpdateDeviceContact(model.Identifier.Value, model.Email, true);

            _logger.LogInformation($"Successfully updated device contact: {model.Identifier}");

            return _mapper.Map<DeviceContactDto>(updatedContact);
        }

        public async Task DeleteDeviceContact(AddOrUpdateDeviceContactDto model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            if (model.Identifier == null)
            {
                throw new ArgumentException("Identifier and DeviceIdentifier are required when deleting a device contact.");
            }
            var existingContact = await _deviceRepository.GetDeviceContact(model.Identifier.Value);

            if (existingContact == null)
            {
                throw new EntityNotFoundException($"Device contact with identifier: {model.Identifier.Value} not found.");
            }

            if (!_userService.HasAccessToDevice(existingContact.Device.Identifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation($"Deleting device contact: {model.Identifier.Value}");

            await _deviceRepository.DeleteDeviceContact(model.Identifier.Value, true);

            _logger.LogInformation($"Successfully deleted device contact: {model.Identifier.Value}");
        }
    }
}
