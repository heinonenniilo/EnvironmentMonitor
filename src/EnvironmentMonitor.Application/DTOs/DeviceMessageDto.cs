using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceMessageDto : IMapFrom<DeviceMessageExtended>
    {
        public bool FirstMessage { get; set; }
        public string? Identifier { get; set; }
        public Guid DeviceIdentifier { get; set; }
        public DateTime TimeStamp { get; set; }
        public long? Uptime { get; set; }
        public long? LoopCount { get; set; }
        public long? MessageCount { get; set; }
        public bool IsDuplicate { get; set; }
        public int? SourceId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceMessageExtended, DeviceMessageDto>()
                .ReverseMap();
        }
    }
}
