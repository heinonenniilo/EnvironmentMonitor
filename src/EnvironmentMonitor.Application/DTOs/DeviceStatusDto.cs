using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceStatusDto : IMapFrom<DeviceStatus>
    {
        public DateTime Timestamp { get; set; }
        public bool Status { get; set; }
        public int DeviceId { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceStatus, DeviceStatusDto>().ForMember(x => x.Timestamp, opt => opt.MapFrom(x => x.TimeStamp));
        }
    }

    public class DeviceStatusModel
    {
        public required List<DeviceStatusDto> DeviceStatuses { get; set; }

        public List<DeviceDto> Devices { get; set; } = [];
    }
}