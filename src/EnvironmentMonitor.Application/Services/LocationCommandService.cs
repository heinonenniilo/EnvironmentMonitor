using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class LocationCommandService : ILocationCommandService
    {
        private readonly ILogger<LocationCommandService> _logger;
        private readonly IUserService _userService;
        private readonly ILocationRepository _locationRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDeviceCommandService _deviceCommandService;

        public LocationCommandService(
            ILogger<LocationCommandService> logger,
            IUserService userService,
            ILocationRepository locationRepository,
            IDeviceRepository deviceRepository,
            IDeviceCommandService deviceCommandService)
        {
            _logger = logger;
            _userService = userService;
            _locationRepository = locationRepository;
            _deviceRepository = deviceRepository;
            _deviceCommandService = deviceCommandService;
        }

        public async Task SetMotionControlStatus(Guid locationIdentifier, MotionControlStatus status, DateTime? triggeringTime = null)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [locationIdentifier], GetDevices = true });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{locationIdentifier}' not found.");

            _logger.LogInformation($"Setting motion control status for location '{locationIdentifier}' with {location.Devices.Count} device(s).");

            var physicalDevices = location.Devices.Where(d => !d.IsVirtual).ToList();

            _logger.LogInformation($"Setting motion control delay for {physicalDevices.Count} physical device(s) in location '{locationIdentifier}'.");

            foreach (var device in physicalDevices)
            {
                await _deviceCommandService.SetMotionControlStatus(device.Identifier, status, triggeringTime);
            }
        }

        public async Task SetMotionControlDelay(Guid locationIdentifier, long delayMs, DateTime? triggeringTime = null)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [locationIdentifier], GetDevices = true });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{locationIdentifier}' not found.");

            _logger.LogInformation($"Setting motion control delay for location '{locationIdentifier}' with {location.Devices.Count} device(s).");
            var physicalDevices = location.Devices.Where(d => !d.IsVirtual).ToList();
            _logger.LogInformation($"Setting motion control delay for {physicalDevices.Count} physical device(s) in location '{locationIdentifier}'.");

            foreach (var device in physicalDevices)
            {
                await _deviceCommandService.SetMotionControlDelay(device.Identifier, delayMs, triggeringTime);
            }
        }
    }
}
