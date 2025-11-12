using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceAttributeDto : IMapFrom<DeviceAttribute>
    {
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public string Type { get; set; }
        public string? Value { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime TimeStampUtc { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceAttribute, DeviceAttributeDto>()
                .ForMember(x => x.TypeName, opt => opt.MapFrom(x => x.Type.Name))
                .ForMember(x => x.TypeDescription, opt => opt.MapFrom(x => x.Type.Description))
                .ForMember(x => x.Type, opt => opt.MapFrom(x => x.Type.Type))
                .ReverseMap();
        }
    }
}
