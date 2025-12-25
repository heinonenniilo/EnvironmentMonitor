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
        private readonly ApplicationSettings _applicationSettings;

        public UserService(
            ICurrentUser currentUser,
            IUserAuthService userAuthService,
            ApplicationSettings applicationSettings)
        {
            _currentUser = currentUser;
            _userAuthService = userAuthService;
            _applicationSettings = applicationSettings;
        }

        public bool HasAccessToDevice(Guid id, AccessLevels accessLevel) => HasAccessTo(EntityRoles.Device, id, accessLevel);

        public bool HasAccessTo(EntityRoles entity, Guid id, AccessLevels accessLevel)
        {
            if (_currentUser?.Claims.Any() != true)
            {
                return false;
            }

            if (HasGlobalRole(GlobalRoles.Admin))
            {
                return true;
            }
            var hasRole = HasGlobalRole(GlobalRoles.Viewer) || _currentUser.Claims.Any(x => x.Type == entity.ToString() && Guid.TryParse(x.Value, out var res) && res == id);
            switch (accessLevel)
            {
                case AccessLevels.None:
                    return false;
                case AccessLevels.Read:
                    return HasGlobalRole(GlobalRoles.Viewer) || hasRole;
                case AccessLevels.Write:
                    return HasGlobalRole(GlobalRoles.Admin);
            };
            return false;
        }

        public bool HasAccessToSensor(Guid id, AccessLevels accessLevel) => HasAccessTo(EntityRoles.Sensor, id, accessLevel);

        public bool HasAccessToSensors(List<Guid> ids, AccessLevels accessLevel) => ids.All(d => HasAccessToSensor(d, accessLevel));
        public bool HasAccessToDevices(List<Guid> ids, AccessLevels accessLevel) => ids.All(d => HasAccessToDevice(d, accessLevel));

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
            model.BaseUrl = _applicationSettings.BaseUrl;
            await _userAuthService.RegisterUser(model);
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            return await _userAuthService.ConfirmEmail(userId, token);
        }

        public List<Guid> GetDevices()
        {
            return _currentUser.Claims.Where(x => x.Type == EntityRoles.Device.ToString()).Select(d => Guid.Parse(d.Value)).ToList();
        }

        public List<Guid> GetLocations()
        {
            return _currentUser.Claims.Where(x => x.Type == EntityRoles.Location.ToString()).Select(d => Guid.Parse(d.Value)).ToList();
        }

        public bool HasAccessToLocations(List<Guid> ids, AccessLevels accessLevel) => ids.All(x => HasAccessTo(EntityRoles.Location, x, accessLevel));

        public bool IsAdmin
        {
            get
            {
                return _currentUser.Roles.Any(r => r.Equals(GlobalRoles.Admin.ToString()));
            }
        }
    }
}
