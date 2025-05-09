using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
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
        public DeviceRepository(MeasurementDbContext context, IDateService dateService, ILogger<DeviceRepository> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
        }
        public async Task<Device?> GetDeviceByIdentifier(string deviceId)
        {
            return await _context.Devices.Include(x => x.Sensors).FirstOrDefaultAsync(x => x.DeviceIdentifier == deviceId);
        }

        public async Task<Device?> GetDeviceByIdentifier(string deviceId, params Expression<Func<Device, object>>[] includes)
        {
            IQueryable<Device> query = _context.Devices;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(x => x.DeviceIdentifier == deviceId);
        }

        public async Task<List<Device>> GetDevices(List<int>? ids = null, bool onlyVisible = true)
        {
            var devices = await _context.Devices.Where(d => (ids == null || ids.Contains(d.Id)) && (!onlyVisible || d.Visible)).ToListAsync();
            return devices;
        }

        public async Task<List<Device>> GetDevices(List<string>? identifiers = null, bool onlyVisible = true)
        {
            var devices = await _context.Devices.Where(d => (identifiers == null || identifiers.Contains(d.DeviceIdentifier)) && (!onlyVisible || d.Visible)).ToListAsync();
            return devices;
        }

        public async Task<Sensor?> GetSensor(int deviceId, int sensorIdInternal)
        {
            var sensor = await _context.Sensors.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.SensorId == sensorIdInternal);
            return sensor;
        }

        public async Task<IEnumerable<Sensor>> GetSensorsByDeviceIdsAsync(List<int> deviceIds)
        {
            var sensors = await _context.Sensors.Where(x => deviceIds.Contains(x.DeviceId)).ToListAsync();
            return sensors;
        }

        public async Task<IEnumerable<Sensor>> GetSensorsByDeviceIdentifiers(List<string> deviceIdentifiers)
        {
            var sensors = await _context.Sensors.Where(x => deviceIdentifiers.Contains(x.Device.DeviceIdentifier)).ToListAsync();
            return sensors;
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

        public async Task<List<DeviceInfo>> GetDeviceInfo(List<int>? ids, bool onlyVisible, bool getAttachments = false)
        {
            IQueryable<Device> query = _context.Devices;
            if (getAttachments)
            {
                query = query.Include(x => x.Attachments).ThenInclude(a => a.Attachment);
            }
            query = query.Include(x => x.Sensors);
            var devices = query.Where(x => ids == null || ids.Contains(x.Id));
            return await GetDeviceInfos(devices);
        }

        public async Task<List<DeviceInfo>> GetDeviceInfo(List<string>? identifiers, bool onlyVisible, bool getAttachments = false)
        {
            IQueryable<Device> query = _context.Devices;
            if (getAttachments)
            {
                query = query.Include(x => x.Attachments).ThenInclude(a => a.Attachment);
            }

            query = query.Include(x => x.Sensors);
            var devices = query.Where(x => identifiers == null || identifiers.Contains(x.DeviceIdentifier));
            return await GetDeviceInfos(devices);
        }

        public async Task<List<DeviceEvent>> GetDeviceEvents(int id)
        {
            var query = _context.DeviceEvents.Where(x => x.DeviceId == id).OrderByDescending(x => x.TimeStamp).Take(100);
            return await query.ToListAsync();
        }

        public async Task<List<DeviceEvent>> GetDeviceEvents(string deviceIdentifier)
        {
            var device = await GetDeviceByIdentifier(deviceIdentifier);
            if (device == null)
            {
                return [];
            }
            var query = _context.DeviceEvents.Include(x => x.Type).Where(x => x.DeviceId == device.Id).OrderByDescending(x => x.TimeStamp).Take(100);
            return await query.ToListAsync();
        }

        public async Task<List<Device>> GetDevicesByLocation(List<int> locationIds)
        {
            return await _context.Devices.Where(x => locationIds.Contains(x.LocationId)).ToListAsync();
        }

        public async Task AddAttachment(int deviceId, Attachment attachment, bool saveChanges)
        {
            var device = await _context.Devices.Include(x => x.Attachments).FirstAsync(x => x.Id == deviceId);
            _context.Attachments.Add(attachment);
            _context.DeviceAttachments.Add(new DeviceAttachment()
            {
                Created = _dateService.CurrentTime(),
                Attachment = attachment,
                Device = device,
                IsDefaultImage = !device.Attachments.Any(x => x.IsDefaultImage)
            });
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Attachment> GetAttachment(int deviceId, Guid attachmentIdentifier)
        {
            var deviceAttachment = await _context.DeviceAttachments.Include(x => x.Attachment).FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Guid == attachmentIdentifier);
            if (deviceAttachment == null)
            {
                throw new EntityNotFoundException();
            }
            return deviceAttachment.Attachment;
        }
        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
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
                DefaultImageGuid = defaultImages.FirstOrDefault(x => x.DeviceId == device.Id)?.Guid
            }).ToList();
        }

        public async Task DeleteAttachment(int deviceId, Guid attachmentIdentifier, bool saveChanges)
        {
            var deviceAttachment = await _context.DeviceAttachments.Include(x => x.Attachment).FirstAsync(x => x.DeviceId == deviceId && x.Guid == attachmentIdentifier);
            _context.Remove(deviceAttachment);
            _context.Remove(deviceAttachment.Attachment);
            if (deviceAttachment.IsDefaultImage)
            {
                var firstOtherAttachment = await _context.DeviceAttachments.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.Guid != attachmentIdentifier);
                if (firstOtherAttachment != null)
                {
                    firstOtherAttachment.IsDefaultImage = true;
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

        public async Task SetStatus(SetDeviceStatusModel model, bool saveChanges)
        {
            bool statusToSet;
            var device = await _context.Devices.FirstOrDefaultAsync(x => x.Id == model.DeviceId) ?? throw new EntityNotFoundException();
            var latestStatus = await _context.DeviceStatusChanges.Where(x => x.DeviceId == device.Id).OrderByDescending(x => x.TimeStamp).FirstOrDefaultAsync();
            var latestMessage = await _context.Measurements.Where(x => x.Sensor.DeviceId == device.Id).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();
            if (model.Status != null)
            {
                statusToSet = model.Status.Value;
            }
            else
            {
                statusToSet = latestMessage != null && ((model.TimeStamp ?? _dateService.CurrentTime()) - latestMessage.Timestamp).TotalMinutes < ApplicationConstants.DeviceWarningLimitInMinutes;
            }
            var timeStamp = model.TimeStamp ?? _dateService.CurrentTime();
            if (latestStatus == null || (latestStatus.Status != statusToSet && timeStamp > latestStatus.TimeStamp))
            {
                _logger.LogInformation($"Device status is being set to: {statusToSet}");
                _context.DeviceStatusChanges.Add(new Domain.Entities.DeviceStatus()
                {

                    Device = device,
                    Status = statusToSet,
                    TimeStamp = timeStamp,
                    TimeStampUtc = _dateService.LocalToUtc(timeStamp),
                    Message = model.Message,
                    DeviceMessage = model.DeviceMessage,
                });
            }
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<DeviceStatus>> GetDevicesStatus(GetDeviceStatusModel model)
        {
            var listToReturn = new List<DeviceStatus>();
            foreach (var deviceId in model.DeviceIds)
            {
                var latestStatusBeforeTimeRangeStart = await _context.DeviceStatusChanges.Where(x => x.DeviceId == deviceId && x.TimeStamp < model.From).OrderBy(x => x.TimeStamp).FirstOrDefaultAsync();
                if (latestStatusBeforeTimeRangeStart != null)
                {
                    listToReturn.Add(latestStatusBeforeTimeRangeStart);
                }
            }

            var query = _context.DeviceStatusChanges.Where(
                x => model.DeviceIds.Contains(x.DeviceId) &&
                x.TimeStamp >= model.From && (model.To == null || x.TimeStamp <= model.To)
            ).OrderBy(x => x.TimeStamp);

            var statusList = await query.ToListAsync();
            listToReturn.AddRange(statusList);

            listToReturn.AddRange(model.DeviceIds.Select(x => new DeviceStatus()
            {
                TimeStamp = _dateService.CurrentTime(),
                Status = statusList.OrderByDescending(x => x.TimeStamp).FirstOrDefault()?.Status ?? false
            }));
            return listToReturn;
        }
    }
}
