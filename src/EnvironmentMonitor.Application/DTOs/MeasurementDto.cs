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
    public class MeasurementDto : MeasurementBaseDto, IMapFrom<Measurement>, IMapFrom<MeasurementExtended>
    {
        public int SensorId { get; set; }
        public Guid SensorGuid { get; set; }
        public new void Mapping(Profile profile)
        {
            profile.CreateMap<Measurement, MeasurementDto>()
                .IncludeBase<Measurement, MeasurementBaseDto>()
                .ReverseMap()
                .IncludeBase<MeasurementBaseDto, Measurement>();

            profile.CreateMap<MeasurementExtended, MeasurementDto>()
                .ReverseMap();
        }
    }

    public class MeasurementBaseDto: IMapFrom<Measurement>, IMapFrom<MeasurementExtended>
    {
        public double SensorValue { get; set; }
        public int TypeId { get; set; }
        public DateTime TimestampUtc { get; set; }
        public DateTime Timestamp { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<Measurement, MeasurementBaseDto>()
                .ForMember(x => x.SensorValue, opt => opt.MapFrom(d => d.Value))
                .ReverseMap();

            profile.CreateMap<MeasurementExtended, MeasurementBaseDto>()
                .ForMember(x => x.SensorValue, opt => opt.MapFrom(d => d.Value))
                .ReverseMap();
        }
    }

    public class MeasurementsBySensorModel
    {
        public List<MeasurementsBySensorDto> Measurements { get; set; } = new List<MeasurementsBySensorDto> { };
        public List<SensorDto> Sensors { get; set; } = new List<SensorDto> { };
    }

    public class MeasurementsModel
    {
        public List<MeasurementDto> Measurements { get; set; } = [];
        public List<MeasurementsInfoDto> MeasurementsInfo { get; set; } = [];
    }

    public class MeasurementsInfoDto
    {
        public Guid SensorId { get; set; }
        public Dictionary<int, MeasurementBaseDto> MinValues { get; set; } = [];
        public Dictionary<int, MeasurementBaseDto> MaxValues { get; set; } = [];
        public Dictionary<int, MeasurementBaseDto> LatestValues { get; set; } = [];
    }

    public class MeasurementsBySensorDto : MeasurementsInfoDto
    {
        public List<MeasurementBaseDto> Measurements { get; set; } = [];
    }

    public class MeasurementsByLocationDto
    {
        public List<MeasurementsBySensorDto> Measurements { get; set; }
        public int Id { get; set; }
        public List<SensorDto> Sensors { get; set; }
    }

    public class MeasurementsByLocationModel
    {
        public List<MeasurementsByLocationDto> Measurements { get; set; } = [];
    }
}
