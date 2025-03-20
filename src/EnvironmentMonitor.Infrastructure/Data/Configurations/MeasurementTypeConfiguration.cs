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

            builder.HasData(Enum.GetValues(typeof(MeasurementTypes))
                .Cast<MeasurementTypes>()
                .Select(x => new MeasurementType
                {
                    Id = (int)x,
                    Name = x.ToString(),
                    Unit = x switch
                    {
                        MeasurementTypes.Temperature => "C",
                        MeasurementTypes.Humidity => "%",
                        MeasurementTypes.Light => "Lx",
                        MeasurementTypes.Motion => "ON/OFF",
                        _ => "-"
                    }
                }).ToList());
        }
    }
}
