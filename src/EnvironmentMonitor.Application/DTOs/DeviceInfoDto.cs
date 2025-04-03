using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Application.ValueResolvers;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceInfoDto : IMapFrom<DeviceInfo>
    {
        public DeviceDto Device { get; set; }
        public DateTime? OnlineSince { get; set; }
        public DateTime? RebootedOn { get; set; }
        public DateTime? LastMessage { get; set; }

        public bool ShowWarning { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceInfo, DeviceInfoDto>().ForMember(x => x.ShowWarning, opt => opt.MapFrom<ShowDeviceWarningResolver>()).ReverseMap();
        }
    }
}
