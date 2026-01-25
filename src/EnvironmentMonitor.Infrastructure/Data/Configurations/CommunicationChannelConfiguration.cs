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
    public class CommunicationChannelConfiguration : IEntityTypeConfiguration<CommunicationChannel>
    {
        public void Configure(EntityTypeBuilder<CommunicationChannel> builder)
        {
            builder.HasKey(cc => cc.Id);
            builder.Property(cc => cc.Id).ValueGeneratedNever();

            builder.Property(cc => cc.Name).IsRequired().HasMaxLength(256);
            builder.Property(cc => cc.Description).HasMaxLength(1024);

            builder.HasMany(cc => cc.Devices)
                .WithOne(d => d.CommunicationChannel)
                .HasForeignKey(d => d.CommunicationChannelId)
                .IsRequired(false);

            // Seed data from CommunicationChannels enum
            var values = Enum.GetValues(typeof(CommunicationChannels)).Cast<CommunicationChannels>().Select(v => new CommunicationChannel
            {
                Id = (int)v,
                Name = v.ToString(),
                Description = GetEnumDescription(v)
            }).ToArray();

            builder.HasData(values);
        }

        private static string GetEnumDescription(CommunicationChannels value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attr = fi.GetCustomAttribute<DescriptionAttribute>();
            return attr != null ? attr.Description : value.ToString();
        }
    }
}
