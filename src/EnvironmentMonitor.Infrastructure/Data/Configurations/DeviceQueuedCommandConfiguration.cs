using EnvironmentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceQueuedCommandConfiguration : IEntityTypeConfiguration<DeviceQueuedCommand>
    {
        public void Configure(EntityTypeBuilder<DeviceQueuedCommand> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.MessageId)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(d => d.Message)
                .IsRequired()
                .HasColumnType("nvarchar(MAX)");

            builder.Property(d => d.Type)
                .IsRequired();

            builder.Property(d => d.DeviceId)
                .IsRequired();

            builder.Property(d => d.Scheduled)
                .IsRequired();

            builder.Property(d => d.ScheduledUtc)
                .IsRequired();

            builder.Property(d => d.Created)
                .IsRequired();

            builder.Property(d => d.CreatedUtc)
                .IsRequired();

            builder.HasIndex(d => d.MessageId).IsUnique();
            builder.HasIndex(d => new { d.DeviceId, d.ScheduledUtc });
            builder.HasIndex(d => new { d.DeviceId, d.ExecutedAtUtc });

            builder.HasOne(d => d.CommandType)
                .WithMany(t => t.QueuedCommands)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
