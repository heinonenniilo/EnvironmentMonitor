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
    public class DeviceEventDto : IMapFrom<DeviceEvent>
    {
        public DateTime TimeStamp { get; set; }
        public int DeviceId { get; set; }
        public string? Message { get; set; }

        public string Type { get; set; }
        public long Id { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceEvent, DeviceEventDto>().ForMember(x => x.Type, opt => opt.MapFrom(x => x.Type != null ? x.Type.Name : "")).ReverseMap();
        }
    }
}
