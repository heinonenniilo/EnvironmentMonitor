using EnvironmentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class SensorTypeConfiguration : IEntityTypeConfiguration<SensorType>
    {
        public void Configure(EntityTypeBuilder<SensorType> builder)
        {
            // Primary Key
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id)
                .ValueGeneratedNever();
            builder.HasIndex(x => x.Name).IsUnique();

            builder.HasMany(x => x.Sensors)
                .WithOne(x => x.Type)
                .HasForeignKey(x => x.TypeId);

            builder.HasData(new List<SensorType>()
            {
                new SensorType()
                {
                    Id = (int)SensorTypes.Undefined, Name = "Undefined", Unit = "-"
                },
                new SensorType() {
                    Id = (int)SensorTypes.Temperature, Name = "Temperature", Unit = "C"
                },
                new SensorType() {
                    Id = (int)SensorTypes.Humidity, Name = "Humidity", Unit = "%"
                }
            }); 
            
        }
    }
}
