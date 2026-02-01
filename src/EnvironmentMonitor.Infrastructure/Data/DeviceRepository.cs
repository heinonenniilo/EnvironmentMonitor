using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.AddModels;
using EnvironmentMonitor.Domain.Models.GetModels;
using EnvironmentMonitor.Domain.Models.Pagination;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Device = EnvironmentMonitor.Domain.Entities.Device;
using DeviceStatus = EnvironmentMonitor.Domain.Entities.DeviceStatus;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly IDateService _dateService;
        private readonly ILogger<DeviceRepository> _logger;
        private readonly IPaginationService _paginationService;
        public DeviceRepository(MeasurementDbContext context, IDateService dateService, IPaginationService paginationService, ILogger<DeviceRepository> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
            _paginationService = paginationService;
        }

        public async Task<List<Device>> GetDevices(GetDevicesModel model)
        {
            IQueryable<Device> query = GetFilteredDeviceQuery(model);
            var devices = await query.ToListAsync();
            return devices;
        }

        public async Task<List<SensorExtended>> GetSensors(GetSensorsModel model)
        {
            var query = _context.Sensors.Where(x =>
                (model.DevicesModel.DeviceIdentifiers == null || model.DevicesModel.DeviceIdentifiers.Contains(x.Device.DeviceIdentifier))
                && (model.DevicesModel.Ids == null || model.DevicesModel.Ids.Contains(x.Device.Id))
                && (model.Identifiers == null || model.Identifiers.Contains(x.Identifier))
                && (model.DevicesModel.Identifiers == null || model.DevicesModel.Identifiers.Contains(x.Device.Identifier))
                );

            if (model.IncludeVirtualSensors)
            {
                query = query.Include(x => x.VirtualSensorRowValues);
                query = query.Include(x => x.VirtualSensorRows);
            }

            if (model.Ids != null)
            {
                query = query.Where(x => model.Ids.Contains(x.Id));
            }
            if (model.SensorIds != null)
            {
                query = query.Where(x => model.SensorIds.Contains(x.SensorId));
            }
            return await query.Select(x => new SensorExtended()
            {
                Id = x.Id,
                SensorId = x.SensorId,
                Name = x.Name,
                Identifier = x.Identifier,
                DeviceId = x.DeviceId,
                DeviceIdentifier = x.Device.Identifier,
                ScaleMin = x.ScaleMin,
                ScaleMax = x.ScaleMax,
                TypeId = x.TypeId,
                VirtualSensorRowValues = x.VirtualSensorRowValues,
                VirtualSensorRows = x.VirtualSensorRows,
            }).ToListAsync();
        }

        public async Task<DeviceEvent> AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges, DateTime? dateTimeUtc)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new ArgumentException($"Could not find a device with id: {deviceId}");
            }
            var utcDate = dateTimeUtc ?? DateTime.UtcNow;
            var toAdd = new DeviceEvent()
            {
                DeviceId = device.Id,
                TypeId = (int)type,
                Message = message,
                TimeStampUtc = utcDate,
                TimeStamp = _dateService.UtcToLocal(utcDate),
            };
            await _context.DeviceEvents.AddAsync(toAdd);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return toAdd;
        }

        public async Task<List<DeviceInfo>> GetDeviceInfo(GetDevicesModel model)
        {
            IQueryable<Device> query = GetFilteredDeviceQuery(model);
            query = query.Include(x => x.Sensors)
                .ThenInclude(s => s.VirtualSensorRows)
                    .ThenInclude(vsr => vsr.ValueSensor)
                        .ThenInclude(hh => hh.Device);
            
            return await GetDeviceInfos(query);
        }

        public async Task<List<DeviceEvent>> GetDeviceEvents(int id)
        {
            var query = _context.DeviceEvents.Include(x => x.Type).Where(x => x.DeviceId == id).OrderByDescending(x => x.TimeStamp).Take(100);
            return await query.ToListAsync();
        }

        public async Task AddAttachment(AddDeviceAttachmentModel model)
        {
            var device = await _context.Devices.Include(x => x.Attachments).FirstAsync(x => x.Identifier == model.Identifier) 
                ?? throw new EntityNotFoundException($"Device with identifier: {model.Identifier} not found.");
            _context.Attachments.Add(model.Attachment);
            _context.DeviceAttachments.Add(new DeviceAttachment()
            {
                Created = _dateService.CurrentTime(),
                Attachment = model.Attachment,
                Device = device,
                IsDefaultImage = model.IsDeviceImage && (!device.Attachments.Any(x => x.IsDefaultImage)),
                IsDeviceImage = model.IsDeviceImage,
            });
            if (model.SaveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Attachment> GetAttachment(int deviceId, Guid attachmentIdentifier)
        {
            var deviceAttachment = await _context.DeviceAttachments.Include(x => x.Attachment).FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Guid == attachmentIdentifier)
                ?? throw new EntityNotFoundException($"Attachment for device with id: {deviceId} and attachment identifier: {attachmentIdentifier} not found.");
            return deviceAttachment.Attachment;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAttachment(int deviceId, Guid attachmentIdentifier, bool saveChanges)
        {
            var deviceAttachment = await _context.DeviceAttachments.Include(x => x.Attachment).FirstAsync(x => x.DeviceId == deviceId && x.Guid == attachmentIdentifier)
                ?? throw new EntityNotFoundException($"Attachment for device with id: {deviceId} and attachment identifier: {attachmentIdentifier} not found.");
            _context.Remove(deviceAttachment);
            _context.Remove(deviceAttachment.Attachment);
            if (deviceAttachment.IsDefaultImage)
            {
                var firstOtherImageAttachment = await _context.DeviceAttachments.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Guid != attachmentIdentifier && x.IsDeviceImage);
                if (firstOtherImageAttachment != null)
                {
                    firstOtherImageAttachment.IsDefaultImage = true;
                }
            }
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetDefaultImage(int deviceId, Guid attachmentIdentifier)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentDefaultImage = await _context.DeviceAttachments.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.IsDefaultImage);
                if (currentDefaultImage != null)
                {
                    currentDefaultImage.IsDefaultImage = false;
                    await _context.SaveChangesAsync();
                }
                var attachmentToSetAsDefault = await _context.DeviceAttachments.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Guid == attachmentIdentifier) ?? throw new InvalidOperationException($"Device attachment '{attachmentIdentifier}' not found");
                attachmentToSetAsDefault.IsDefaultImage = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Setting default image failed");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<DeviceStatus?> SetStatus(SetDeviceStatusModel model, bool saveChanges)
        {
            DeviceStatus? updatedStatus = null;
            bool statusToSet;
            var device = await _context.Devices.FirstOrDefaultAsync(x => x.Identifier == model.Idenfifier) ?? throw new EntityNotFoundException();
            var latestStatus = await _context.DeviceStatusChanges.Where(x => x.DeviceId == device.Id).OrderByDescending(x => x.TimeStamp).FirstOrDefaultAsync();

            var latestMeasurement = await _context.Measurements.Where(x => 
                x.Sensor.DeviceId == device.Id
                && x.Timestamp > _dateService.CurrentTime().AddDays(-1) // Optimization
            ).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();

            var latestDeviceMessage = await _context.DeviceMessages.Where(x =>
                x.DeviceId == device.Id
                && x.TimeStamp > _dateService.CurrentTime().AddDays(-1) 
            ).OrderByDescending(x => x.TimeStamp).FirstOrDefaultAsync();

            // Determine the latest timestamp from both sources
            DateTime? latestActivityTimestamp = null;
            if (latestMeasurement != null && latestDeviceMessage != null)
            {
                latestActivityTimestamp = latestMeasurement.Timestamp > latestDeviceMessage.TimeStamp 
                    ? latestMeasurement.Timestamp 
                    : latestDeviceMessage.TimeStamp;
            }
            else if (latestMeasurement != null)
            {
                latestActivityTimestamp = latestMeasurement.Timestamp;
            }
            else if (latestDeviceMessage != null)
            {
                latestActivityTimestamp = latestDeviceMessage.TimeStamp;
            }

            if (model.Status != null)
            {
                statusToSet = model.Status.Value;
            }
            else
            {
                statusToSet = latestActivityTimestamp != null && ((model.TimeStamp ?? _dateService.CurrentTime()) - latestActivityTimestamp.Value).TotalMinutes < ApplicationConstants.DeviceWarningLimitInMinutes;
            }
            var timeStamp = model.TimeStamp ?? _dateService.CurrentTime();
            if (latestStatus == null || (latestStatus.Status != statusToSet && timeStamp > latestStatus.TimeStamp))
            {
                _logger.LogInformation($"Device status is being set to: {statusToSet}");
                updatedStatus = new DeviceStatus()
                {
                    Device = device,
                    Status = statusToSet,
                    TimeStamp = timeStamp,
                    TimeStampUtc = _dateService.LocalToUtc(timeStamp),
                    Message = model.Message,
                    DeviceMessage = model.DeviceMessage,
                };
                _context.DeviceStatusChanges.Add(updatedStatus);
            }
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return updatedStatus;
        }

        public async Task<List<DeviceStatus>> GetDevicesStatus(GetDeviceStatusModel model)
        {
            var listToReturn = new List<DeviceStatus>();

            var devices = await _context.Devices.Where(x => model.DeviceIdentifiers.Contains(x.Identifier)).ToListAsync();
            var deviceIds = devices.Select(x => x.Id).ToList();

            if (model.LatestOnly)
            {
                foreach (var deviceId in deviceIds)
                {
                    var latestStatus = await _context.DeviceStatusChanges.Where(x => x.DeviceId == deviceId).OrderByDescending(x => x.TimeStamp).FirstOrDefaultAsync();
                    if (latestStatus != null)
                    {
                        listToReturn.Add(latestStatus);
                    }
                }
                return listToReturn;
            }

            foreach (var deviceId in deviceIds)
            {
                var latestStatusBeforeTimeRangeStart = await _context.DeviceStatusChanges.Where(x => x.DeviceId == deviceId && x.TimeStamp < model.From).OrderByDescending(x => x.TimeStamp).FirstOrDefaultAsync();
                if (latestStatusBeforeTimeRangeStart != null)
                {
                    listToReturn.Add(new DeviceStatus()
                    {
                        DeviceId = latestStatusBeforeTimeRangeStart.DeviceId,
                        TimeStamp = model.From,
                        Status = latestStatusBeforeTimeRangeStart.Status,
                    });
                }
            }

            var query = _context.DeviceStatusChanges.Where(
                x => deviceIds.Contains(x.DeviceId) &&
                x.TimeStamp >= model.From && (model.To == null || x.TimeStamp <= model.To)
            ).OrderBy(x => x.TimeStamp);

            var statusList = await query.ToListAsync();
            listToReturn.AddRange(statusList);
            listToReturn.AddRange(deviceIds.Select(x => new DeviceStatus()
            {
                TimeStamp = _dateService.CurrentTime(),
                Status = listToReturn.Where(y => y.DeviceId == x).OrderByDescending(x => x.TimeStamp).FirstOrDefault()?.Status ?? false,
                DeviceId = x
            }));
            return listToReturn;
        }

        public async Task<DeviceInfo> AddOrUpdate(Device device, bool saveChanges)
        {
            var deviceInDb = await _context.Devices.FirstOrDefaultAsync(x => x.Id == device.Id);
            Device deviceToUpdate;
            if (device.Id > 0 && deviceInDb == null)
            {
                throw new EntityNotFoundException();
            }
            if (deviceInDb != null)
            {
                deviceToUpdate = deviceInDb;
            }
            else
            {
                deviceToUpdate = new Device() { Name = device.Name, DeviceIdentifier = device.DeviceIdentifier };
            }

            deviceToUpdate.Name = string.IsNullOrEmpty(device.Name) ? deviceToUpdate.Name : device.Name;
            deviceToUpdate.DeviceIdentifier = string.IsNullOrEmpty(device.DeviceIdentifier) ? deviceToUpdate.DeviceIdentifier : device.DeviceIdentifier;
            deviceToUpdate.Visible = device.Visible;
            deviceToUpdate.CommunicationChannelId = device.CommunicationChannelId;

            if (deviceInDb == null)
            {
                _logger.LogInformation($"Adding device named: '{deviceToUpdate.Name}'");
                _context.Devices.Add(deviceToUpdate);
            }
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            var toReturn = (await GetDeviceInfo(new GetDevicesModel()
            {
                Ids = [deviceToUpdate.Id],
                GetAttachments = true,
                OnlyVisible = false
            }))
            .FirstOrDefault();
            return toReturn ?? throw new EntityNotFoundException();
        }

        public async Task<PaginatedResult<DeviceMessageExtended>> GetDeviceMessages(GetDeviceMessagesModel model)
        {
            IQueryable<DeviceMessage> query = _context.DeviceMessages;
            if (model.DeviceIdentifiers != null)
            {
                query = query.Where(x => model.DeviceIdentifiers.Contains(x.Device.Identifier));
            }

            if (model.LocationIdentifiers != null)
            {
                query = query.Where(x => model.LocationIdentifiers.Contains(x.Device.Location.Identifier));
            }

            if (model.IsDuplicate != null)
            {
                query = query.Where(x => x.IsDuplicate == model.IsDuplicate);
            }

            if (model.IsFirstMessage != null)
            {
                query = query.Where(x => x.FirstMessage == model.IsFirstMessage);
            }

            if (model.From != null)
            {
                query = query.Where(x => x.TimeStamp >= model.From);
            }

            if (model.To != null)
            {
                query = query.Where(x => x.TimeStamp < model.To);
            }

            var extendedQuery = query.Select(x => new DeviceMessageExtended()
            {
                Id = x.Id,
                DeviceId = x.DeviceId,
                DeviceIdentifier = x.Device.Identifier,
                TimeStamp = x.TimeStamp,
                TimeStampUtc = x.TimeStampUtc,
                FirstMessage = x.FirstMessage,
                Identifier = x.Identifier,
                MessageCount = x.MessageCount,
                IsDuplicate = x.IsDuplicate,
                Created = x.Created,
                CreatedUtc = x.CreatedUtc,
                SourceId = x.SourceId
            });

            var res = await _paginationService.PaginateAsync(extendedQuery, new PaginationParams()
            {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                OrderBy = model.OrderBy ?? "Timestamp",
                IsDescending = model.IsDescending,
            });
            return res;
        }

        private IQueryable<Device> GetFilteredDeviceQuery(GetDevicesModel model)
        {
            IQueryable<Device> query = _context.Devices;

            if (model.GetSensors)
            {
                query = query.Include(x => x.Sensors);
            }

            if (model.GetAttachments)
            {
                query = query.Include(x => x.Attachments).ThenInclude(a => a.Attachment);
            }

            if (model.GetLocation)
            {
                query = query.Include(x => x.Location);
            }

            if (model.GetContacts)
            {
                query = query.Include(x => x.Contacts);
            }

            if (model.GetAttributes)
            {
                query = query.Include(x => x.DeviceAttributes).ThenInclude(a => a.Type);
            }

            if (model.Ids != null)
            {
                query = query.Where(x => model.Ids.Contains(x.Id));
            }
            if (model.DeviceIdentifiers != null)
            {
                query = query.Where(x => model.DeviceIdentifiers.Contains(x.DeviceIdentifier));
            }

            if (model.Identifiers != null)
            {
                query = query.Where(x => model.Identifiers.Contains(x.Identifier));
            }

            if (model.OnlyVisible)
            {
                query = query.Where(x => x.Visible);
            }

            if (model.LocationIdentifiers != null)
            {
                query = query.Where(x => model.LocationIdentifiers.Contains(x.Location.Identifier));
            }
            
            if (model.CommunicationChannelIds != null)
            {
                query = query.Where(x => x.CommunicationChannelId != null && model.CommunicationChannelIds.Contains(x.CommunicationChannelId.Value));
            }
            
            return query;
        }

        private async Task<List<DeviceInfo>> GetDeviceInfos(IEnumerable<Device> devices)
        {
            var returnList = new List<DeviceInfo>();
            var deviceIds = devices.Select(x => x.Id).ToList();
            var query = await _context.DeviceEvents.Where(x => deviceIds.Contains(x.DeviceId)).GroupBy(x => new { x.DeviceId, x.TypeId }).Select(x => new
            {
                x.Key.DeviceId,
                x.Key.TypeId,
                TimeStamp = x.Max(d => d.TimeStamp)
            }).ToListAsync();


            var latestMessages = await _context.Measurements.Where(
                x => deviceIds.Contains(x.Sensor.DeviceId)
                && x.Timestamp > _dateService.CurrentTime().AddDays(-1 * ApplicationConstants.DeviceLastMessageFetchLimitIndays)
             ).GroupBy(x => x.Sensor.DeviceId).Select(d => new { DeviceId = d.Key, Latest = d.Max(x => x.Timestamp) }).ToListAsync();


            var defaultImages = await _context.DeviceAttachments.Where(x => x.IsDefaultImage && deviceIds.Contains(x.DeviceId)).ToListAsync();
            return devices.Select(device => new DeviceInfo()
            {
                Device = device,
                OnlineSince = query.FirstOrDefault(x => x.DeviceId == device.Id && x.TypeId == (int)DeviceEventTypes.Online)?.TimeStamp,
                RebootedOn = query.FirstOrDefault(x => x.DeviceId == device.Id && x.TypeId == (int)DeviceEventTypes.RebootCommand)?.TimeStamp,
                LastMessage = latestMessages.FirstOrDefault(x => x.DeviceId == device.Id)?.Latest,
                DefaultImageGuid = defaultImages.FirstOrDefault(x => x.DeviceId == device.Id)?.Guid,
            }).ToList();
        }

        public async Task SetDeviceAttributes(int deviceId, List<DeviceAttribute> attributes, bool saveChanges)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: {deviceId} not found.");
            }

            var existingAttributes = await _context.DeviceAttributes
                .Where(x => x.DeviceId == deviceId)
                .ToListAsync();

            var attributeTypeIds = attributes.Select(x => x.TypeId).ToList();

            var attributesToRemove = existingAttributes
                .Where(x => !attributeTypeIds.Contains(x.TypeId))
                .ToList();

            if (attributesToRemove.Any())
            {
                _context.DeviceAttributes.RemoveRange(attributesToRemove);
            }

            foreach (var attribute in attributes)
            {
                var existingAttribute = existingAttributes
                    .FirstOrDefault(x => x.TypeId == attribute.TypeId);

                if (existingAttribute != null)
                {
                    // Update existing attribute
                    existingAttribute.Value = attribute.Value;
                    existingAttribute.TimeStamp = attribute.TimeStamp;
                    existingAttribute.TimeStampUtc = attribute.TimeStampUtc;
                }
                else
                {
                    // Add new attribute
                    var newAttribute = new DeviceAttribute
                    {
                        DeviceId = deviceId,
                        TypeId = attribute.TypeId,
                        Value = attribute.Value,
                        TimeStamp = attribute.TimeStamp,
                        TimeStampUtc = attribute.TimeStampUtc,
                        Created = _dateService.CurrentTime(),
                        CreatedUtc = DateTime.UtcNow
                    };
                    await _context.DeviceAttributes.AddAsync(newAttribute);
                }
            }

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateDeviceAttribute(int deviceId, int typeId, string value, bool saveChanges)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: {deviceId} not found.");
            }

            var existingAttribute = await _context.DeviceAttributes
                .FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.TypeId == typeId);

            var currentTime = _dateService.CurrentTime();
            var utcTime = _dateService.LocalToUtc(currentTime);

            if (existingAttribute != null)
            {
                // Update existing attribute
                existingAttribute.Value = value;
                existingAttribute.TimeStamp = currentTime;
                existingAttribute.TimeStampUtc = utcTime;

                existingAttribute.Updated = currentTime;
                existingAttribute.UpdatedUtc = utcTime;
            }
            else
            {
                // Add new attribute
                var newAttribute = new DeviceAttribute
                {
                    DeviceId = deviceId,
                    TypeId = typeId,
                    Value = value,
                    TimeStamp = currentTime,
                    TimeStampUtc = utcTime,
                    Created = _dateService.CurrentTime(),
                    CreatedUtc = utcTime
                };
                await _context.DeviceAttributes.AddAsync(newAttribute);
            }

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<DeviceAttribute>> GetDeviceAttributes(int deviceId)
        {
            return await _context.DeviceAttributes
                .Include(x => x.Type)
                .Where(x => x.DeviceId == deviceId)
                .OrderBy(x => x.TypeId)
                .ToListAsync();
        }

        public async Task<DeviceAttribute?> GetDeviceAttribute(int deviceId, int typeId)
        {
            return await _context.DeviceAttributes
                .Include(x => x.Type)
                .FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.TypeId == typeId);
        }

        public async Task<List<DeviceQueuedCommand>> GetQueuedCommands(GetQueuedCommandsModel model)
        {
            IQueryable<DeviceQueuedCommand> query = _context.DeviceQueuedCommands;

            if (model.Ids != null && model.Ids.Any())
            {
                query = query.Where(x => model.Ids.Contains(x.Id));
            }

            if (model.DeviceIds != null && model.DeviceIds.Any())
            {
                query = query.Where(x => model.DeviceIds.Contains(x.DeviceId));
            }

            if (model.DeviceIdentifiers != null && model.DeviceIdentifiers.Any())
            {
                query = query.Where(x => model.DeviceIdentifiers.Contains(x.Device.Identifier));
            }

            if (model.MessageIds != null && model.MessageIds.Any())
            {
                query = query.Where(x => model.MessageIds.Contains(x.MessageId));
            }

            if (model.ScheduledFrom != null)
            {
                query = query.Where(x => x.ScheduledUtc >= model.ScheduledFrom.Value);
            }

            if (model.ScheduledTo != null)
            {
                query = query.Where(x => x.ScheduledUtc <= model.ScheduledTo.Value);
            }

            if (model.IsExecuted != null)
            {
                if (model.IsExecuted.Value)
                {
                    query = query.Where(x => x.ExecutedAtUtc != null);
                }
                else
                {
                    query = query.Where(x => x.ExecutedAtUtc == null);
                }
            }
            query = query.Include(x => x.CommandType);
            query = query.Include(x => x.OriginalCommand);

            query = query.OrderByDescending(x => x.ScheduledUtc);

            if (model.Limit != null && model.Limit > 0 && model.Limit < 100)
            {
                query = query.Take(model.Limit.Value);
            }
            else
            {
                query = query.Take(50);
            }

            query = query.Include(x => x.Device);

            return await query.ToListAsync();
        }

        public async Task SetQueuedCommand(int deviceId, DeviceQueuedCommand command, bool saveChanges)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: {deviceId} not found.");
            }

            // Check if a command with the same MessageId already exists
            var existingCommand = await _context.DeviceQueuedCommands
                .FirstOrDefaultAsync(x => x.MessageId == command.MessageId);
            if (existingCommand != null)
            {
                existingCommand = command;
                var updatedTime = _dateService.CurrentTime();
                existingCommand.Updated = updatedTime;
                existingCommand.UpdatedUtc = _dateService.LocalToUtc(updatedTime);
                _logger.LogInformation($"Updated queued command with MessageId: {command.MessageId} for device: {deviceId}");
            }
            else
            {
                // Add new command
                command.DeviceId = deviceId;
                command.Created = _dateService.CurrentTime();
                command.CreatedUtc = DateTime.UtcNow;

                await _context.DeviceQueuedCommands.AddAsync(command);
                _logger.LogInformation($"Added new queued command with MessageId: {command.MessageId} for device: {deviceId}");
            }

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DeviceContact> AddDeviceContact(int deviceId, string email, bool saveChanges)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: {deviceId} not found.");
            }

            // Check if contact with same email already exists for this device
            var existingContact = await _context.DeviceContacts
                .FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Email == email);

            if (existingContact != null)
            {
                throw new InvalidOperationException($"Contact with email '{email}' already exists for this device.");
            }

            var created = _dateService.CurrentTime();

            var contact = new DeviceContact
            {
                DeviceId = deviceId,
                Email = email,
                Created = created,
                CreatedUtc = _dateService.LocalToUtc(created),
            };

            await _context.DeviceContacts.AddAsync(contact);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            return contact;
        }

        public async Task<DeviceContact> UpdateDeviceContact(Guid identifier, string email, bool saveChanges)
        {
            var contact = await _context.DeviceContacts
                .FirstOrDefaultAsync(x => x.Identifier == identifier);

            if (contact == null)
            {
                throw new EntityNotFoundException($"Device contact with identifier: {identifier} not found.");
            }

            // Check if another contact with the same email already exists for this device
            var existingContact = await _context.DeviceContacts
                .FirstOrDefaultAsync(x => x.DeviceId == contact.DeviceId && x.Email == email && x.Identifier != identifier);

            if (existingContact != null)
            {
                throw new InvalidOperationException($"Another contact with email '{email}' already exists for this device.");
            }

            contact.Email = email;

            var now = _dateService.CurrentTime();
            contact.Updated = now;
            contact.UpdatedUtc = _dateService.LocalToUtc(now);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            return contact;
        }

        public async Task DeleteDeviceContact(Guid identifier, bool saveChanges)
        {
            var contact = await _context.DeviceContacts
                .FirstOrDefaultAsync(x => x.Identifier == identifier);

            if (contact == null)
            {
                throw new EntityNotFoundException($"Device contact with identifier: {identifier} not found.");
            }

            _context.DeviceContacts.Remove(contact);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DeviceContact?> GetDeviceContact(Guid identifier)
        {
            return await _context.DeviceContacts
                .Include(x => x.Device)
                .FirstOrDefaultAsync(x => x.Identifier == identifier);
        }
    }
}
