using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceEmailTemplateDto : IMapFrom<DeviceEmailTemplate>
    {
        public Guid Identifier { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceEmailTemplate, DeviceEmailTemplateDto>()
                .ReverseMap();
        }
    }
}
