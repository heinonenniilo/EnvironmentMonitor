using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceQueuedCommandTypeConfiguration : IEntityTypeConfiguration<DeviceQueuedCommandType>
    {
        public void Configure(EntityTypeBuilder<DeviceQueuedCommandType> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(d => d.Description)
                .IsRequired()
                .HasMaxLength(1024);

            builder.HasIndex(d => d.Name).IsUnique();

            builder.HasMany(d => d.QueuedCommands)
                .WithOne(s => s.CommandType)
                .HasForeignKey(s => s.Type)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(Enum.GetValues(typeof(QueuedMessages))
                .Cast<QueuedMessages>()
                .Select(e => new DeviceQueuedCommandType
                {
                    Id = (int)e,
                    Name = e.ToString(),
                    Description = e.GetDescription()
                })
                .ToList());
        }
    }
}
