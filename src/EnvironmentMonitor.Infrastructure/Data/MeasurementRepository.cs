using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class MeasurementRepository : IMeasurementRepository
    {
        private readonly MeasurementDbContext _context;
        public MeasurementRepository(MeasurementDbContext context)
        {
            _context = context;
        }
        public async Task<Device?> GetDeviceByIdAsync(string deviceId)
        {
            return await _context.Devices.FirstOrDefaultAsync(x => x.DeviceIdentifier == deviceId);
        }

        public async Task<Sensor?> GetSensor(int deviceId, int sensorIdInternal)
        {
            var sensor = await _context.Sensors.FirstOrDefaultAsync(x => x.DeviceId == deviceId && x.SensorId == sensorIdInternal);
            return sensor;
        }

        public async Task<IEnumerable<Measurement>> GetMeasurements(GetMeasurementsModel model)
        {
            return await _context.Measurements
                .Where(x =>
                x.SensorId == model.SensorId
                && x.Timestamp >= model.From
                && (model.To == null || x.Timestamp <= model.To))
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sensor>> GetSensorsByDeviceIdAsync(string deviceId)
        {
            var device = await GetDeviceByIdAsync(deviceId);
            return await _context.Sensors.Where(x => x.DeviceId == device.Id).ToListAsync();
        }

        public async Task<IList<Measurement>> AddMeasurements(List<Measurement> measurements)
        {
            await _context.Measurements.AddRangeAsync(measurements);
            await _context.SaveChangesAsync();
            return measurements;
        }

        public async Task<MeasurementType?> GetMeasurementType(int id)
        {
            var type = await _context.MeasurementTypes.FirstOrDefaultAsync(x => x.Id == id);
            return type;
        }
    }
}
