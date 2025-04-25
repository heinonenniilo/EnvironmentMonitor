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
    public class DeviceDto : IMapFrom<Device>
    {
        public int Id { get; set; }
        public string DeviceIdentifier { get; set; }
        public string Name { get; set; }
        public List<SensorDto> Sensors { get; set; } = [];
        public bool Visible { get; set; }
        public bool HasMotionSensor { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Device, DeviceDto>()
                .ReverseMap();
        }
    }
}
