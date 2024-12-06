using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserAuthService _userAuthService;

        public UserService(
            ICurrentUser currentUser,
            IUserAuthService userAuthService)
        {
            _currentUser = currentUser;
            _userAuthService = userAuthService;
        }

        public bool HasAccessToDevice(int id, AccessLevels accessLevel)
        {
            if (_currentUser?.Claims.Any() != true)
            {
                return false;
            }

            if (HasGlobalRole(GlobalRoles.Admin))
            {
                return true;
            }

            switch (accessLevel)
            {
                case AccessLevels.None:
                    return false;
                case AccessLevels.Read:
                    return HasGlobalRole(GlobalRoles.Viewer) || _currentUser.Claims.Any(x => x.Type == EntityRoles.Device.ToString() && int.TryParse(x.Value, out var res) && res == id);
                case AccessLevels.Write:
                    return HasGlobalRole(GlobalRoles.Admin);
            };
            return false;
        }

        public bool HasAccessToSensor(int id, AccessLevels accessLevel)
        {
            if (_currentUser?.Claims.Any() != true)
            {
                return false;
            }

            if (HasGlobalRole(GlobalRoles.Admin))
            {
                return true;
            }

            switch (accessLevel)
            {
                case AccessLevels.None:
                    return false;
                case AccessLevels.Read:
                    return HasGlobalRole(GlobalRoles.Viewer) || _currentUser.Claims.Any(x => x.Type == EntityRoles.Sensor.ToString() && int.TryParse(x.Value, out var res) && res == id);
                case AccessLevels.Write:
                    return HasGlobalRole(GlobalRoles.Admin);
            };
            return false;
        }

        public bool HasAccessToSensors(List<int> ids, AccessLevels accessLevel) => ids.All(d => HasAccessToSensor(d, accessLevel));
        public bool HasAccessToDevices(List<int> ids, AccessLevels accessLevel) => ids.All(d => HasAccessToDevice(d, accessLevel));

        private bool HasGlobalRole(GlobalRoles roles)
        {
            return _currentUser.Roles.Any(d => d.Equals(roles.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task Login(LoginModel model)
        {
            await _userAuthService.Login(model);
        }

        public async Task ExternalLogin(ExternalLoginModel model)
        {
            await _userAuthService.LoginWithExternalProvider(model);
        }

        public async Task RegisterUser(RegisterUserModel model)
        {
            await _userAuthService.RegisterUser(model);
        }

        public List<int> GetDevices()
        {
            return _currentUser.Claims.Where(x => x.Type == EntityRoles.Device.ToString()).Select(d => int.Parse(d.Value)).ToList();
        }

        public bool IsAdmin
        {
            get
            {
                return _currentUser.Roles.Any(r => r.Equals(GlobalRoles.Admin.ToString()));
            }
        }
    }
}
