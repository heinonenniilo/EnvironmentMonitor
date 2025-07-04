﻿using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.GetModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface ILocationRepository
    {
        public Task<List<LocationSensor>> GetLocationSensors(List<int> locationIds);
        public Task<List<Location>> GetLocations(GetLocationsModel model);
    }
}
