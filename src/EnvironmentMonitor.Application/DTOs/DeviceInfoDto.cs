using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class DeviceInfoDto : IMapFrom<Device>
    {
        public DeviceDto Device { get; set; }
        public DateTime? OnlineSince { get; set; }
        public DateTime? RebootedOn { get; set; }
        public DateTime? LastMessage { get; set; }

        public bool ShowWarning { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<DeviceInfo, DeviceInfoDto>().ForMember(x => x.ShowWarning, opt => opt.MapFrom(x => x.LastMessageUtc == null || x.LastMessageUtc < DateTime.UtcNow.AddMinutes(-10))).ReverseMap();
        }
    }
}
