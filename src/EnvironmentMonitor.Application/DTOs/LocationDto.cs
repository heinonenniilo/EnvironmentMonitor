using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using System.Text.Json.Serialization;

namespace EnvironmentMonitor.Application.DTOs
{
    public class LocationDto : EntityDto, IMapFrom<Location>
    {
        public bool Visible { get; set; }
        public List<SensorDto> LocationSensors { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<DeviceDto>? Devices { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Location, LocationDto>()
                .ForMember(x => x.DisplayName, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.Devices, opt => opt.MapFrom(x => x.Devices))
                .ReverseMap();
        }
    }
}
