using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class LocationService: ILocationService
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILocationRepository _locationRepository;
        public LocationService(IUserService userService, ILocationRepository locationRepository, IMapper mapper)
        {
            _userService = userService;
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        public async Task<List<LocationDto>> GetLocations()
        {
            var locations = await _locationRepository.GetLocations(_userService.IsAdmin ? null : _userService.GetLocations(), true);
            return _mapper.Map<List<LocationDto>>(locations);
        }
    }
}
