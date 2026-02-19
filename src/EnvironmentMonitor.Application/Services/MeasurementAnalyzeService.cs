using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models.ReturnModel;

namespace EnvironmentMonitor.Application.Services
{
    public class MeasurementAnalyzeService : IMeasurementAnalyzeService
    {
        private readonly IMapper _mapper;

        public MeasurementAnalyzeService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public List<MeasurementsInfoDto> GetMeasurementInfo(ICollection<MeasurementExtended> measurements, List<Guid> sensorIds)
        {
            var returnList = new List<MeasurementsInfoDto>();
            foreach (var sensorId in sensorIds)
            {
                var measurementsToCheck = measurements.Where(x => x.SensorIdentifier == sensorId).ToList();
                if (!measurementsToCheck.Any())
                    continue;
                var rowToAdd = new MeasurementsInfoDto() { SensorIdentifier = sensorId };
                foreach (MeasurementTypes type in Enum.GetValues(typeof(MeasurementTypes)))
                {
                    if (measurementsToCheck.Any(x => x.TypeId == (int)type && x.SensorIdentifier == sensorId))
                    {
                        rowToAdd.MinValues[(int)type] = _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderBy(x => x.Value).First());
                        rowToAdd.MaxValues[(int)type] = _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.Value).First());
                        rowToAdd.LatestValues[(int)type] = _mapper.Map<Measurement, MeasurementDto>(measurementsToCheck.Where(x => x.TypeId == (int)type).OrderByDescending(x => x.Timestamp).First());
                    }
                }
                returnList.Add(rowToAdd);
            }
            return returnList;
        }
    }
}
