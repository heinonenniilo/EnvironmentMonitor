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
    public class MeasurementDbContext : IdentityDbContext<ApplicationUser, ApplicationUserRole, string>
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeasurementDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
