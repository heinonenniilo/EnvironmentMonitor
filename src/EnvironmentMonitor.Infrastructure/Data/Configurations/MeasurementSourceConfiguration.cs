using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using System.ComponentModel;
using System.Reflection;
using System;
using System.Linq;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class MeasurementSourceConfiguration : IEntityTypeConfiguration<MeasurementSource>
    {
        public void Configure(EntityTypeBuilder<MeasurementSource> builder)
        {
            builder.HasKey(ms => ms.Id);
            builder.Property(ms => ms.Id).ValueGeneratedNever();

            builder.Property(ms => ms.Name).IsRequired().HasMaxLength(256);
            builder.Property(ms => ms.Description).HasMaxLength(1024);

            builder.HasMany(ms => ms.DeviceMessages)
                .WithOne(dm => dm.MeasurementSource)
                .HasForeignKey(dm => dm.SourceId)
                .IsRequired(false);

            // Seed data from MeasurementSourceTypes enum
            var values = Enum.GetValues(typeof(MeasurementSourceTypes)).Cast<MeasurementSourceTypes>().Select(v => new MeasurementSource
            {
                Id = (int)v,
                Name = v.ToString(),
                Description = GetEnumDescription(v)
            }).ToArray();

            builder.HasData(values);
        }

        private static string GetEnumDescription(MeasurementSourceTypes value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attr = fi.GetCustomAttribute<DescriptionAttribute>();
            return attr != null ? attr.Description : value.ToString();
        }
    }
}
