using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models.ReturnModel;

namespace EnvironmentMonitor.Application.ValueResolvers
{
    public class ShowSensorWarningResolver : IValueResolver<SensorExtended, SensorInfoDto, bool>
    {
        private readonly IDateService _dateService;

        public ShowSensorWarningResolver(IDateService dateService)
        {
            _dateService = dateService;
        }

        public bool Resolve(SensorExtended source, SensorInfoDto destination, bool destMember, ResolutionContext context)
        {
            if (source.LastMeasurement == null)
            {
                return true;
            }

            var limitInMinutes = source.IsVirtual
                ? ApplicationConstants.VirtualDeviceWarningLimitInMinutes
                : ApplicationConstants.DeviceWarningLimitInMinutes;

            return _dateService.CurrentTime().AddMinutes(-1 * limitInMinutes) > source.LastMeasurement;
        }
    }
}
