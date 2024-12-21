﻿using EnvironmentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.DeviceIdentifier).IsRequired();
            builder.HasIndex(x => x.DeviceIdentifier).IsUnique();

            builder.HasIndex(d => d.Name).IsUnique();

            builder.HasMany(d => d.Sensors)
                .WithOne(s => s.Device)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Events).WithOne(x => x.Device).HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
