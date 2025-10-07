using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceAttachmentConfiguration : IEntityTypeConfiguration<DeviceAttachment>
    {
        public void Configure(EntityTypeBuilder<DeviceAttachment> builder)
        {
            builder.HasKey(x => new { x.DeviceId, x.AttachmentId });
            builder.HasOne(x => x.Device).WithMany(x => x.Attachments).HasForeignKey(x => x.DeviceId);
            builder.HasOne(x => x.Attachment).WithMany(x => x.DeviceAttachments).HasForeignKey(x => x.AttachmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.DeviceId }).IsUnique().HasFilter("[IsDefaultImage] = 1");
            builder.Property(x => x.IsDefaultImage).IsRequired();
            builder.HasIndex(x => x.Guid).IsUnique();
            builder.Property(x => x.Guid).IsRequired();
            builder.Property(x => x.Guid).HasDefaultValueSql("newid()");
        }
    }
}
