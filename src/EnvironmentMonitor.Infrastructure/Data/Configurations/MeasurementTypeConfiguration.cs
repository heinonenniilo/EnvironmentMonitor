using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class MeasurementTypeConfiguration : IEntityTypeConfiguration<MeasurementType>
    {
        public void Configure(EntityTypeBuilder<MeasurementType> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(m => m.Name).IsRequired();
            builder.Property(m => m.Unit).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasMany(x => x.Measurements).WithOne(x => x.Type).HasForeignKey(x => x.TypeId).IsRequired();

            builder.HasData(new List<MeasurementType>()
            {
                new MeasurementType()
                {
                    Id = (int)MeasurementTypes.Undefined, Name = "Undefined", Unit = "-"
                },
                new MeasurementType() {
                    Id = (int)MeasurementTypes.Temperature, Name = "Temperature", Unit = "C"
                },
                new MeasurementType() {
                    Id = (int)MeasurementTypes.Humidity, Name = "Humidity", Unit = "%"
                }
            });

        }
    }
}
