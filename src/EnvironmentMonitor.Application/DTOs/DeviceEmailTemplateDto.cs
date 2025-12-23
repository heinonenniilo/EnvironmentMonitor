using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceEmailTemplateDto : EntityDto, IMapFrom<DeviceEmailTemplate>
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceEmailTemplate, DeviceEmailTemplateDto>()
                .ForMember(x => x.DisplayName, opt => opt.MapFrom(x => ((DeviceEmailTemplateTypes)x.Id).ToString()));
        }
    }
}
