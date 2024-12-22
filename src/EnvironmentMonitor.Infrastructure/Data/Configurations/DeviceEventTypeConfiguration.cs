using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Extensions;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceEventTypeConfiguration : IEntityTypeConfiguration<DeviceEventType>
    {
        public void Configure(EntityTypeBuilder<DeviceEventType> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(512);

            builder.HasIndex(d => d.Name).IsUnique();

            builder.HasMany(d => d.Events)
                .WithOne(s => s.Type)
                .HasForeignKey(s => s.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(Enum.GetValues(typeof(DeviceEventTypes))
                .Cast<DeviceEventTypes>()
                .Select(e => new DeviceEventType
                {
                    Id = (int)e,
                    Name = e.ToString(),
                    Description = e.GetDescription()
                })
                .ToList());
        }
    }
}
