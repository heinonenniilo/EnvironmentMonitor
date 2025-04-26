using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.ValueResolvers
{
    public class ShowDeviceWarningResolver : IValueResolver<DeviceInfo, DeviceInfoDto, bool>
    {
        private readonly IDateService _dateService;

        public ShowDeviceWarningResolver(IDateService dateService)
        {
            _dateService = dateService;
        }

        public bool Resolve(DeviceInfo source, DeviceInfoDto destination, bool destMember, ResolutionContext context)
        {
            return source.LastMessage == null || _dateService.CurrentTime().AddMinutes(-1 * ApplicationConstants.DeviceWarningLimitInMinutes) > source.LastMessage;
        }
    }
}
