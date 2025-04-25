using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Azure.Devices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Device = EnvironmentMonitor.Domain.Entities.Device;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly IDateService _dateService;
        public DeviceRepository(MeasurementDbContext context, IDateService dateService)
        {
            _context = context;
            _dateService = dateService;
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
            _context.DeviceAttachments.Add(new DeviceAttachment() { Attachment = attachment, Device = device, IsDefaultImage = !device.Attachments.Any(x => x.IsDefaultImage) });
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

            return devices.Select(device => new DeviceInfo()
            {
                Device = device,
                OnlineSince = query.FirstOrDefault(x => x.DeviceId == device.Id && x.TypeId == (int)DeviceEventTypes.Online)?.TimeStamp,
                RebootedOn = query.FirstOrDefault(x => x.DeviceId == device.Id && x.TypeId == (int)DeviceEventTypes.RebootCommand)?.TimeStamp,
                LastMessage = latestMessages.FirstOrDefault(x => x.DeviceId == device.Id)?.Latest
            }).ToList();
        }

        public async Task DeleteAttachment(int deviceId, Guid attachmentIdentifier, bool saveChanges)
        {
            var deviceAttachment = await _context.DeviceAttachments.Include(x => x.Attachment).FirstAsync(x => x.DeviceId == deviceId && x.Guid == attachmentIdentifier);
            _context.Remove(deviceAttachment);
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
    }
}
