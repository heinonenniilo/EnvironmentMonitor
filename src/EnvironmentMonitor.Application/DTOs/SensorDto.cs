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
    public class SensorDto : IMapFrom<Sensor>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SensorId { get; set; }
        public int DeviceId { get; set; }
        public double? ScaleMin { get; set; }
        public double? ScaleMax { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Sensor, SensorDto>().ReverseMap();
            profile.CreateMap<LocationSensor, SensorDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.SensorId))
                .ForMember(x => x.ScaleMin, opt => opt.MapFrom(x => x.Sensor != null ? x.Sensor.ScaleMin : null))
                .ForMember(x => x.ScaleMax, opt => opt.MapFrom(x => x.Sensor != null ? x.Sensor.ScaleMax : null))
                .ReverseMap();
        }
    }
}
