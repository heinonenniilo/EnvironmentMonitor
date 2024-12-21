using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
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
            return await _context.Devices.FirstOrDefaultAsync(x => x.DeviceIdentifier == deviceId);
        }

        public async Task<List<Device>> GetDevices(List<int>? ids = null)
        {
            var devices = await _context.Devices.Where(d => ids == null || ids.Contains(d.Id)).ToListAsync();
            return devices;
        }

        public async Task<List<Device>> GetDevices(List<string>? identifiers = null)
        {
            var devices = await _context.Devices.Where(d => identifiers == null || identifiers.Contains(d.DeviceIdentifier)).ToListAsync();
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
    }
}
