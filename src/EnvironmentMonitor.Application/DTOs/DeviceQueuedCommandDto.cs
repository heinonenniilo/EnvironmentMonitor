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
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public bool IsRemoved { get; set; }
        public int? OriginalId { get; set; }
        public string? OriginalMessageId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceQueuedCommand, DeviceQueuedCommandDto>()
                .ForMember(dest => dest.DeviceIdentifier, opt => opt.MapFrom(src => src.Device.Identifier))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.CommandType.Name))
                .ForMember(dest => dest.OriginalMessageId, opt => opt.MapFrom(src => src.OriginalCommand != null ? src.OriginalCommand.MessageId : null))
                .ReverseMap();
        }
    }
}
