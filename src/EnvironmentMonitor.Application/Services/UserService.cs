using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserAuthService _userAuthService;
        private readonly ApplicationSettings _applicationSettings;
        private readonly IQueueClient _queueClient;

        public UserService(
            ICurrentUser currentUser,
            IUserAuthService userAuthService,
            ApplicationSettings applicationSettings,
            IQueueClient queueClient)
        {
            _currentUser = currentUser;
            _userAuthService = userAuthService;
            _applicationSettings = applicationSettings;
            _queueClient = queueClient;
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
            model.BaseUrl = !string.IsNullOrEmpty(_applicationSettings.BaseUrl) 
                ? $"{_applicationSettings.BaseUrl.TrimEnd('/')}/api/authentication/confirm-email" 
                : "/api/authentication/confirm-email";
            await _userAuthService.RegisterUser(model);
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            return await _userAuthService.ConfirmEmail(userId, token);
        }

        public async Task ChangePassword(ChangePasswordModel model)
        {
            var userId = _currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            
            await _userAuthService.ChangePassword(userId, model);
        }

        public async Task ForgotPassword(ForgotPasswordModel model)
        {
            if (model.Enqueue)
            {
                var attributesToAdd = new Dictionary<string, string>()
                    {
                        { ApplicationConstants.QueuedMessageDefaultKey, model.Email }
                    };
                var messageToQueue = new DeviceQueueMessage()
                {
                    Attributes = attributesToAdd,
                    DeviceIdentifier = Guid.Empty,
                    MessageTypeId = (int)QueuedMessages.ProcessForgetUserPasswordRequest,
                };
                var messageJson = JsonSerializer.Serialize(messageToQueue);
                var res = await _queueClient.SendMessage(messageJson);
                return;
            }

            model.BaseUrl = !string.IsNullOrEmpty(_applicationSettings.BaseUrl)
                ? $"{_applicationSettings.BaseUrl.TrimEnd('/')}/reset-password"
                : "/reset-password";
            await _userAuthService.ForgotPassword(model);
        }

        public async Task<bool> ResetPassword(ResetPasswordModel model)
        {
            return await _userAuthService.ResetPassword(model);
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

        public async Task DeleteOwnUser()
        {
            var userId = _currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            await _userAuthService.DeleteUser(userId);
            await _userAuthService.Logout();
        }

        public async Task DeleteUser(string userId)
        {
            if (!IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can delete other users");
            }

            await _userAuthService.DeleteUser(userId);
        }

        public async Task<List<UserInfoDto>> GetAllUsers()
        {
            if (!IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can list users");
            }

            var users = await _userAuthService.GetAllUsers();
            return users.Select(u => new UserInfoDto
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                EmailConfirmed = u.EmailConfirmed,
                Roles = u.Roles,
                Claims = u.Claims.Select(c => new UserClaimDto
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList(),
                LockoutEnd = u.LockoutEnd,
                LockoutEnabled = u.LockoutEnabled,
                AccessFailedCount = u.AccessFailedCount
            }).ToList();
        }

        public async Task<UserInfoDto?> GetUser(string userId)
        {
            if (!IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can view user details");
            }

            var user = await _userAuthService.GetUser(userId);
            if (user == null)
            {
                return null;
            }

            return new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                Roles = user.Roles,
                Claims = user.Claims.Select(c => new UserClaimDto
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList(),
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount
            };
        }
    }
}