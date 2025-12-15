using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class MeasurementDbContext : DbContext
    {
        public MeasurementDbContext(DbContextOptions<MeasurementDbContext> options)
            : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveMaxLength(256);
        }

        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<MeasurementType> MeasurementTypes { get; set; }
        public DbSet<DeviceEventType> DeviceEventTypes { get; set; }
        public DbSet<DeviceEvent> DeviceEvents { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationSensor> LocationSensors { get; set; }
        public DbSet<PublicSensor> PublicSensors { get; set; }
        public DbSet<DeviceAttachment> DeviceAttachments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<DeviceStatus> DeviceStatusChanges { get; set; }
        public DbSet<DeviceMessage> DeviceMessages { get; set; }
        public DbSet<VirtualSensorRow> VirtualSensorRows { get; set; }
        public DbSet<DeviceAttributeType> DeviceAttributeTypes { get; set; }
        public DbSet<DeviceAttribute> DeviceAttributes { get; set; }
        public DbSet<DeviceQueuedCommand> DeviceQueuedCommands { get; set; }
        public DbSet<DeviceQueuedCommandType> DeviceQueuedCommandTypes { get; set; }
        public DbSet<DeviceEmailTemplate> DeviceEmailTemplates { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeasurementDbContext).Assembly, type =>
                type.Namespace == "EnvironmentMonitor.Infrastructure.Data.Configurations");
            base.OnModelCreating(modelBuilder);
        }
    }
}