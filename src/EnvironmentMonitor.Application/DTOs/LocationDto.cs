using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Application.DTOs
{
    public class LocationDto : IMapFrom<Location>
    {
        public Guid Identifier { get; set; }
        public required string Name { get; set; }
        public bool Visible { get; set; }
        public List<SensorDto> LocationSensors { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Location, LocationDto>().ReverseMap();
        }
    }
}
