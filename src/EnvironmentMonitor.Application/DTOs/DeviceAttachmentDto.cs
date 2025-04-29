using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceAttachmentDto : IMapFrom<DeviceAttachment>
    {
        public Guid Guid { get; set; }
        public bool IsImage { get; set; }
        public bool IsDefaultImage { get; set; }
        public DateTime Created { get; set; }
        public string? Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceAttachment, DeviceAttachmentDto>().ForMember(x => x.Name, opt => opt.MapFrom(x => x.Attachment != null ? x.Attachment.OriginalName : "")).ReverseMap();
        }
    }
}
