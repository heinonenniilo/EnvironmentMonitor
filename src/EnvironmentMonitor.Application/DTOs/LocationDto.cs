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
    public class LocationDto : IMapFrom<Location>
    {
        public string Name { get; set; }
        public List<SensorDto> LocationSensors { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Location, LocationDto>().ReverseMap();
        }
    }
}
