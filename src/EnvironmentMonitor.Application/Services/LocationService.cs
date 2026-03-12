using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILocationRepository _locationRepository;
        private readonly IDeviceRepository _deviceRepository;

        public LocationService(IUserService userService, ILocationRepository locationRepository, IDeviceRepository deviceRepository, IMapper mapper)
        {
            _userService = userService;
            _locationRepository = locationRepository;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
        }

        public async Task<List<LocationDto>> GetLocations(GetLocationsModel model)
        {
            if (!_userService.IsAdmin)
            {
                if (model.Identifiers != null && model.Identifiers.Count > 0)
                {
                    if (!_userService.HasAccessToLocations(model.Identifiers, AccessLevels.Read))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                else
                {
                    model.Identifiers = _userService.GetLocations();
                }
            }
            model.IncludeLocationSensors = true;
            var locations = await _locationRepository.GetLocations(model);
            return _mapper.Map<List<LocationDto>>(locations);
        }

        public async Task<LocationDto> AddLocation(AddLocationDto model)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            var location = new Location
            {
                Name = model.Name,
            };

            var added = await _locationRepository.AddLocation(location, true);
            return _mapper.Map<LocationDto>(added);
        }

        public async Task DeleteLocation(Guid locationIdentifier)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [locationIdentifier] });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{locationIdentifier}' not found.");

            await _locationRepository.DeleteLocation(location.Id, true);
        }

        public async Task<LocationDto> AddLocationSensor(AddOrUpdateLocationSensorDto model)
        {
            if (!_userService.HasAccessToLocations([model.LocationIdentifier], AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [model.LocationIdentifier] });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{model.LocationIdentifier}' not found.");

            var sensors = await _deviceRepository.GetSensors(new GetSensorsModel
            {
                DevicesModel = new GetDevicesModel(),
                Identifiers = [model.SensorIdentifier]
            });
            var sensor = sensors.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Sensor with identifier: '{model.SensorIdentifier}' not found.");

            if (!_userService.HasAccessToDevice(sensor.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            await _locationRepository.AddLocationSensor(location.Id, sensor.Id, sensor.DeviceId, model.Name, model.TypeId, true);

            return await GetLocationDto(location.Id);
        }

        public async Task<LocationDto> UpdateLocationSensor(AddOrUpdateLocationSensorDto model)
        {
            if (!_userService.HasAccessToLocations([model.LocationIdentifier], AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [model.LocationIdentifier] });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{model.LocationIdentifier}' not found.");

            var sensors = await _deviceRepository.GetSensors(new GetSensorsModel
            {
                DevicesModel = new GetDevicesModel(),
                Identifiers = [model.SensorIdentifier]
            });
            var sensor = sensors.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Sensor with identifier: '{model.SensorIdentifier}' not found.");

            if (!_userService.HasAccessToDevice(sensor.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            await _locationRepository.UpdateLocationSensor(location.Id, sensor.Id, sensor.DeviceId, model.Name, model.TypeId, true);

            return await GetLocationDto(location.Id);
        }

        public async Task<LocationDto> DeleteLocationSensor(AddOrUpdateLocationSensorDto model)
        {
            if (!_userService.HasAccessToLocations([model.LocationIdentifier], AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [model.LocationIdentifier] });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{model.LocationIdentifier}' not found.");

            var sensors = await _deviceRepository.GetSensors(new GetSensorsModel
            {
                DevicesModel = new GetDevicesModel(),
                Identifiers = [model.SensorIdentifier]
            });
            var sensor = sensors.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Sensor with identifier: '{model.SensorIdentifier}' not found.");

            if (!_userService.HasAccessToDevice(sensor.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            await _locationRepository.DeleteLocationSensor(location.Id, sensor.Id, sensor.DeviceId, true);

            return await GetLocationDto(location.Id);
        }

        public async Task<LocationDto> UpdateLocation(LocationDto model)
        {
            if (!_userService.HasAccessToLocations([model.Identifier], AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [model.Identifier] });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{model.Identifier}' not found.");

            location.Name = model.Name;
            location.Visible = model.Visible;

            await _locationRepository.UpdateLocation(location, true);

            return await GetLocationDto(location.Id);
        }

        public async Task MoveDevicesToLocation(MoveDevicesToLocationDto model)
        {
            if (!_userService.HasAccessToLocations([model.LocationIdentifier], AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            if (!_userService.HasAccessToDevices(model.DeviceIdentifiers, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var locations = await _locationRepository.GetLocations(new GetLocationsModel { Identifiers = [model.LocationIdentifier] });
            var location = locations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with identifier: '{model.LocationIdentifier}' not found.");

            var devices = await _deviceRepository.GetDevices(new GetDevicesModel { Identifiers = model.DeviceIdentifiers });
            if (devices.Count != model.DeviceIdentifiers.Count)
            {
                throw new EntityNotFoundException("One or more devices not found.");
            }

            await _locationRepository.MoveDevicesToLocation(location.Id, devices.Select(d => d.Id).ToList(), true);
        }

        private async Task<LocationDto> GetLocationDto(int locationId)
        {
            var updatedLocations = await _locationRepository.GetLocations(new GetLocationsModel { Ids = [locationId], IncludeLocationSensors = true });
            var updatedLocation = updatedLocations.FirstOrDefault()
                ?? throw new EntityNotFoundException($"Location with id: {locationId} not found.");
            return _mapper.Map<LocationDto>(updatedLocation);
        }
    }
}
