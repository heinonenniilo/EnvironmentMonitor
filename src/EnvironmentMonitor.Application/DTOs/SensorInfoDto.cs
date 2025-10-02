using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class SensorInfoDto : SensorDto, IMapFrom<Sensor>, IMapFrom<SensorExtended>
    {
        public int SensorId { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMap<Sensor, SensorInfoDto>()
                .IncludeBase<Sensor, SensorDto>()
                .ReverseMap();

            profile.CreateMap<SensorExtended, SensorInfoDto>()
                .IncludeBase<SensorExtended, SensorDto>()
                .ReverseMap();
        }
    }
}
