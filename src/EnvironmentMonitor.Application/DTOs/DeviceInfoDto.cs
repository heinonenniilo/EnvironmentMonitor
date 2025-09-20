using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Application.ValueResolvers;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceInfoDto : IMapFrom<DeviceInfo>
    {
        public DeviceDto Device { get; set; }
        public DateTime? OnlineSince { get; set; }
        public DateTime? RebootedOn { get; set; }
        public DateTime? LastMessage { get; set; }
        public List<DeviceAttachmentDto> Attachments { get; set; } = [];
        public Guid? DefaultImageGuid { get; set; }
        public string DeviceIdentifier { get; set; }

        public bool ShowWarning { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceInfo, DeviceInfoDto>()
                .ForMember(x => x.ShowWarning, opt => opt.MapFrom<ShowDeviceWarningResolver>())
                .ForMember(x => x.Attachments, opt => opt.MapFrom(x => x.Device.Attachments ?? new List<DeviceAttachment>()))
                .ForMember(x => x.DeviceIdentifier, opt => opt.MapFrom(x => x.Device.DeviceIdentifier))
                .ReverseMap();
        }
    }
}
