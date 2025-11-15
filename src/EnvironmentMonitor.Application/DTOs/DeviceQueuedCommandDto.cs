using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using System;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceQueuedCommandDto : IMapFrom<DeviceQueuedCommand>
    {
        public string MessageId { get; set; } = string.Empty;
        public Guid DeviceIdentifier { get; set; }
        public DateTime Scheduled { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public int Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Created { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceQueuedCommand, DeviceQueuedCommandDto>()
                .ForMember(dest => dest.DeviceIdentifier, opt => opt.MapFrom(src => src.Device.Identifier))
                .ReverseMap();
        }
    }
}
