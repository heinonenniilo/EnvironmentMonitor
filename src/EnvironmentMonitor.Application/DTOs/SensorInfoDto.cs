using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Application.ValueResolvers;
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
        public List<VirtualSensorRowDto> Sensors { get; set; } = [];
        public bool IsVirtual { get; set; }
        public bool Active { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? LastMeasurement { get; set; }
        public bool ShowWarning { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMap<Sensor, SensorInfoDto>()               
                .IncludeBase<Sensor, SensorDto>()
                .ForMember(x => x.Sensors, opt => opt.MapFrom(x => x.VirtualSensorRows))
                .ForMember(x => x.Active, opt => opt.MapFrom(x => x.Active))
                .ForMember(x => x.LastMeasurement, opt => opt.Ignore() )               
                .ReverseMap();

            profile.CreateMap<SensorExtended, SensorInfoDto>()
                .IncludeBase<SensorExtended, SensorDto>()
                .ForMember(x => x.Sensors, opt => opt.MapFrom(x => x.VirtualSensorRows))
                .ForMember(x => x.Active, opt => opt.MapFrom(x => x.Active))
                .ForMember(x => x.ShowWarning, opt => opt.MapFrom<ShowSensorWarningResolver>())
                .ReverseMap();
        }
    }
}
