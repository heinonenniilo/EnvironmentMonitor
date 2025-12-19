using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceContactDto : IMapFrom<DeviceContact>
    {
        public Guid Identifier { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public DateTime CreatedUtc { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceContact, DeviceContactDto>()
                .ReverseMap();
        }
    }
}
