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

        public long? SizeInBytes { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceAttachment, DeviceAttachmentDto>()
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Attachment != null ? x.Attachment.OriginalName : ""))
                .ForMember(x => x.SizeInBytes, opt => opt.MapAtRuntime())
                .ForMember(x => x.IsImage, opt => opt.MapFrom(x => x.Attachment.IsImage))
                .ReverseMap();
        }
    }
}
