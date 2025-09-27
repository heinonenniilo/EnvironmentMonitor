﻿using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IUserService
    {
        public Task Login(LoginModel model);
        public Task ExternalLogin(ExternalLoginModel model);
        public Task RegisterUser(RegisterUserModel model);
        bool HasAccessTo(EntityRoles entity, Guid id, AccessLevels accessLevel);

        bool HasAccessToDevice(Guid id, AccessLevels accessLevel);
        bool HasAccessToSensor(Guid id, AccessLevels accessLevel);
        bool HasAccessToSensors(List<Guid> ids, AccessLevels accessLevel);
        bool HasAccessToDevices(List<Guid> ids, AccessLevels accessLevel);
        bool HasAccessToLocations(List<Guid> ids, AccessLevels accessLevel);
        public List<Guid> GetDevices();
        public List<Guid> GetLocations();
        public bool IsAdmin { get; }
    }
}
