using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class SensorDto : EntityDto, IMapFrom<Sensor>, IMapFrom<SensorExtended>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? DeviceIdentifier { get; set; }
        public double? ScaleMin { get; set; }
        public double? ScaleMax { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? LocationIdentifier { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MeasurementType { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<Sensor, SensorDto>()
                .ForMember(x => x.DeviceIdentifier, opt => opt.MapFrom(d => d.Device != null ? d.Device.Identifier : (Guid?)null))
                .ReverseMap();
            profile.CreateMap<SensorExtended, SensorDto>().ReverseMap();

            profile.CreateMap<LocationSensor, SensorDto>()
                .ForMember(x => x.Identifier, opt => opt.MapFrom(x => x.Sensor.Identifier))
                .ForMember(x => x.ScaleMin, opt => opt.MapFrom(x => x.Sensor != null ? x.Sensor.ScaleMin : null))
                .ForMember(x => x.ScaleMax, opt => opt.MapFrom(x => x.Sensor != null ? x.Sensor.ScaleMax : null))
                .ForMember(x => x.LocationIdentifier, opt => opt.MapFrom(x => x.Location != null ? x.Location.Identifier : (Guid?)null))
                .ForMember(x => x.MeasurementType, opt => opt.MapFrom(x => x.TypeId))
                .ReverseMap();
            profile.CreateMap<PublicSensor, SensorDto>()
                .ForMember(x => x.ScaleMin, opt => opt.MapFrom(x => x.Sensor != null ? x.Sensor.ScaleMin : null))
                .ForMember(x => x.ScaleMax, opt => opt.MapFrom(x => x.Sensor != null ? x.Sensor.ScaleMax : null))
                .ForMember(x => x.MeasurementType, opt => opt.MapFrom(x => x.TypeId))
                .ReverseMap();
        }
    }
}
