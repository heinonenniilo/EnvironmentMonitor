using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Models.ReturnModel;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IMeasurementAnalyzeService
    {
        List<MeasurementsInfoDto> GetMeasurementInfo(ICollection<MeasurementExtended> measurements, List<Guid> sensorIds);
    }
}
