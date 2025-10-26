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
    public class VirtualSensorRowDto : IMapFrom<VirtualSensorRow>
    {
        public Guid Idenfitifier { get; set; }
        public SensorDto Sensor { get; set; }
        public int? TypeId { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<VirtualSensorRow, VirtualSensorRowDto>()
                .ForMember(x => x.Idenfitifier, opt => opt.MapFrom(x => x.VirtualSensor.Identifier))
                .ForMember(x => x.TypeId, opt => opt.MapFrom(x => x.TypeId))
                .ForMember(x => x.Sensor, opt => opt.MapFrom(x => x.ValueSensor))
                .ReverseMap();
        }
    }
}
