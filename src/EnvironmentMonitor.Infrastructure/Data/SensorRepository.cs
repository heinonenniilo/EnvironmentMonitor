using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class SensorRepository : ISensorRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly ILogger<SensorRepository> _logger;

        public SensorRepository(MeasurementDbContext context, ILogger<SensorRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Sensor?> GetSensor(Guid identifier)
        {
            return await _context.Sensors
                .Include(x => x.Device)
                .Include(x => x.VirtualSensorRows)
                    .ThenInclude(x => x.ValueSensor)
                .FirstOrDefaultAsync(x => x.Identifier == identifier);
        }

        public async Task<Sensor?> GetSensor(int id)
        {
            return await _context.Sensors
                .Include(x => x.Device)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Sensor>> GetSensorsByDevice(int deviceId, bool? isActive = null)
        {
            var query = _context.Sensors
                .Include(x => x.Device)
                .Where(x => x.DeviceId == deviceId);

            if (isActive != null)
            {
                query = query.Where(x => x.Active == isActive.Value);
            }

            return await query
                .OrderBy(x => x.SensorId)
                .ToListAsync();
        }

        public async Task<Sensor> AddSensor(Sensor sensor, bool saveChanges)
        {
            var device = await _context.Devices.FindAsync(sensor.DeviceId);
            if (device == null)
            {
                throw new EntityNotFoundException($"Device with id: {sensor.DeviceId} not found.");
            }

            var existingSensor = await _context.Sensors
                .FirstOrDefaultAsync(x => x.DeviceId == sensor.DeviceId && x.SensorId == sensor.SensorId);
            if (existingSensor != null)
            {
                throw new DuplicateEntityException($"Sensor with SensorId: {sensor.SensorId} already exists on device: {sensor.DeviceId}.");
            }

            await _context.Sensors.AddAsync(sensor);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Added sensor '{sensor.Name}' (SensorId: {sensor.SensorId}) to device: {sensor.DeviceId}");
            return sensor;
        }

        public async Task<Sensor> UpdateSensor(Sensor sensor, bool saveChanges)
        {
            var existingSensor = await _context.Sensors
                .FirstOrDefaultAsync(x => x.Identifier == sensor.Identifier);

            if (existingSensor == null)
            {
                throw new EntityNotFoundException($"Sensor with identifier: {sensor.Identifier} not found.");
            }

            existingSensor.Name = sensor.Name;
            existingSensor.ScaleMin = sensor.ScaleMin;
            existingSensor.ScaleMax = sensor.ScaleMax;
            existingSensor.Active = sensor.Active;

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Updated sensor '{existingSensor.Name}' (Identifier: {existingSensor.Identifier})");
            return existingSensor;
        }

        public async Task DeleteSensor(Guid identifier, bool saveChanges)
        {
            var sensor = await _context.Sensors
                .Include(x => x.Measurements)
                .Include(x => x.LocationSensors)
                .Include(x => x.VirtualSensorRows)
                .Include(x => x.VirtualSensorRowValues)
                .FirstOrDefaultAsync(x => x.Identifier == identifier);

            if (sensor == null)
            {
                throw new EntityNotFoundException($"Sensor with identifier: {identifier} not found.");
            }

            if (sensor.Measurements.Count > 0)
            {
                throw new InvalidOperationException($"Cannot delete sensor with identifier: {identifier} because it has {sensor.Measurements.Count} measurements.");
            }

            if (sensor.LocationSensors.Count > 0)
            {
                throw new InvalidOperationException($"Cannot delete sensor with identifier: {identifier} because it is linked to {sensor.LocationSensors.Count} location(s).");
            }

            if (sensor.VirtualSensorRows.Count > 0 || sensor.VirtualSensorRowValues.Count > 0)
            {
                throw new InvalidOperationException($"Cannot delete sensor with identifier: {identifier} because it is linked to virtual sensor configurations.");
            }

            _context.Sensors.Remove(sensor);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Deleted sensor '{sensor.Name}' (Identifier: {identifier})");
        }

        public async Task AddVirtualSensorRow(VirtualSensorRow row, bool saveChanges)
        {
            var existing = await _context.VirtualSensorRows
                .FirstOrDefaultAsync(x => x.VirtualSensorId == row.VirtualSensorId && x.ValueSensorId == row.ValueSensorId);

            if (existing != null)
            {
                throw new DuplicateEntityException($"A VirtualSensorRow for sensor value id {row.ValueSensorId} already exists on virtual sensor id {row.VirtualSensorId}.");
            }

            await _context.VirtualSensorRows.AddAsync(row);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Added VirtualSensorRow: VirtualSensorId={row.VirtualSensorId}, ValueSensorId={row.ValueSensorId}");
        }

        public async Task DeleteVirtualSensorRow(int virtualSensorId, int valueSensorId, bool saveChanges)
        {
            var row = await _context.VirtualSensorRows
                .FirstOrDefaultAsync(x => x.VirtualSensorId == virtualSensorId && x.ValueSensorId == valueSensorId);

            if (row == null)
            {
                throw new EntityNotFoundException($"VirtualSensorRow for virtual sensor id {virtualSensorId} and value sensor id {valueSensorId} not found.");
            }

            _context.VirtualSensorRows.Remove(row);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Deleted VirtualSensorRow: VirtualSensorId={virtualSensorId}, ValueSensorId={valueSensorId}");
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
