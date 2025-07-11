﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(512);
            builder.Property(x => x.Identifier).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.Identifier).IsRequired();
            builder.HasMany(x => x.Devices).WithOne(x => x.Location);
            builder.HasIndex(d => d.Name).IsUnique();
            builder.HasIndex(d => d.Identifier).IsUnique();
            builder.HasMany(x => x.LocationSensors).WithOne(x => x.Location);
            builder.Property(x => x.Visible).HasDefaultValue(true);
        }
    }
}
