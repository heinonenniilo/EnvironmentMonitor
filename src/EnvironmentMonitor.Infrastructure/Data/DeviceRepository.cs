using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly MeasurementDbContext _context;
        public DeviceRepository(MeasurementDbContext context)
        {
            _context = context;
        }
        public async Task<Device?> GetDeviceByIdentifier(string deviceId)
        {
            return await _context.Devices.Include(x => x.Sensors).FirstOrDefaultAsync(x => x.DeviceIdentifier == deviceId);
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
                throw new ArgumentException("Not found");
            }
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ApplicationConstants.TargetTimeZone);
            var utcDate = dateTimeUtc ?? DateTime.UtcNow;
            var toAdd = new DeviceEvent()
            {
                DeviceId = device.Id,
                TypeId = (int)type,
                Message = message,
                TimeStampUtc = utcDate,
                TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(utcDate, TimeZoneInfo.FindSystemTimeZoneById(ApplicationConstants.TargetTimeZone)),
            };
            await _context.DeviceEvents.AddAsync(toAdd);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return toAdd;
        }

        public async Task<List<DeviceInfo>> GetDeviceInfo(List<int>? ids, bool onlyVisible)
        {
            var devices = _context.Devices.Include(x => x.Sensors).Where(x => ids == null || ids.Contains(x.Id));
            return await GetDeviceInfos(devices);
        }

        public async Task<List<DeviceInfo>> GetDeviceInfo(List<string>? identifiers, bool onlyVisible)
        {
            var devices = _context.Devices.Include(x => x.Sensors).Where(x => identifiers == null || identifiers.Contains(x.DeviceIdentifier));
            return await GetDeviceInfos(devices);
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
            return devices.Select(device => new DeviceInfo()
            {
                Device = device,
                OnlineSince = query.FirstOrDefault(x => x.DeviceId == device.Id && x.TypeId == (int)DeviceEventTypes.Online)?.TimeStamp,
                RebootedOn = query.FirstOrDefault(x => x.DeviceId == device.Id && x.TypeId == (int)DeviceEventTypes.RebootCommand)?.TimeStamp
            }).ToList();
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
    }
}
