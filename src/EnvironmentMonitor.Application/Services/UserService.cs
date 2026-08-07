using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IUserCookieService _userCookieService;
        private readonly IHangfireJobService _hangfireJobService;

        public UserService(
            ICurrentUser currentUser,
            IUserAuthService userAuthService,
            ApplicationSettings applicationSettings,
            IQueueClient queueClient,
            IMapper mapper,
            IUserCookieService userCookieService,
            IHangfireJobService hangfireJobService)
        {
            _currentUser = currentUser;
            _userAuthService = userAuthService;
            _applicationSettings = applicationSettings;
            _queueClient = queueClient;
            _mapper = mapper;
            _userCookieService = userCookieService;
            _hangfireJobService = hangfireJobService;
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

            if (entity == EntityRoles.Device)
            {
                if (HasGlobalRole(GlobalRoles.MeasurementWriter))
                {
                    return true;
                }
                // Handle consolidated claims - split values by semicolon
                var hasDeviceWriterRole = _currentUser.Claims
                    .Where(x => x.Type == EntityRoles.DeviceWriter.ToString())
                    .Any(x => x.Value.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Any(v => Guid.TryParse(v, out var res) && res == id));
                if (hasDeviceWriterRole)
                {
                    return true;
                }
            }

            // Handle consolidated claims - split values by semicolon
            var hasRole = HasGlobalRole(GlobalRoles.Viewer) || 
                _currentUser.Claims
                    .Where(x => x.Type == entity.ToString())
                    .Any(x => x.Value.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Any(v => Guid.TryParse(v, out var res) && res == id));
            switch (accessLevel)
            {
                case AccessLevels.None:
                    return false;
                case AccessLevels.Read:
                    return HasGlobalRole(GlobalRoles.Viewer) || hasRole;
                case AccessLevels.Write:
                    return HasGlobalRole(GlobalRoles.Admin);
            }
            ;
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
            // Write auth success cookie with "Local" as the login provider for username/password login
            _userCookieService.WriteAuthSuccessCookie("Local");
        }

        public async Task<ExternalLoginResult> ExternalLogin(ExternalLoginModel model)
        {
            var result = await _userAuthService.LoginWithExternalProvider(model);

            if (!result.Success)
            {
                // Write auth failure to cookie that JavaScript can read
                _userCookieService.WriteAuthFailureCookie(result.Errors, result.ErrorCode, result.LoginProvider);
            }
            else
            {
                // Write auth success to cookie with the external login provider
                _userCookieService.WriteAuthSuccessCookie(result.LoginProvider);
            }

            return result;
        }

        public async Task RegisterUser(RegisterUserModel model)
        {
            model.BaseUrl = !string.IsNullOrEmpty(model.BaseUrl) ? model.BaseUrl : _applicationSettings.BaseUrl;
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
                // Try to enqueue with Hangfire first if available
                if (_hangfireJobService.IsAvailable)
                {
                    var baseUrl = string.IsNullOrEmpty(model.BaseUrl) ? _applicationSettings.BaseUrl : model.BaseUrl;
                    model.BaseUrl = baseUrl;

                    _hangfireJobService.Enqueue<IUserAuthService>(service => service.ForgotPassword(model));
                    return;
                }

                // Fallback to queue client if Hangfire is not available
                var attributesToAdd = new Dictionary<string, string>()
                {
                        {ApplicationConstants.QueuedMessageDefaultKey, model.Email },
                        {ApplicationConstants.QueuedMessageApplicationBaseUrlKey, model.BaseUrl ?? "" }
                };

                var messageToQueue = new DeviceQueueMessage()
                {
                    Attributes = attributesToAdd,
                    DeviceIdentifier = Guid.Empty,
                    MessageTypeId = (int)QueuedMessages.ProcessForgetUserPasswordRequest,
                };
                var messageJson = JsonSerializer.Serialize(messageToQueue);
                await _queueClient.SendMessage(messageJson);
                return;
            }
            model.BaseUrl = string.IsNullOrEmpty(model.BaseUrl) ? _applicationSettings.BaseUrl : model.BaseUrl;
            await _userAuthService.ForgotPassword(model);
        }

        public async Task<bool> ResetPassword(ResetPasswordModel model)
        {
            return await _userAuthService.ResetPassword(model);
        }

        public List<Guid> GetDevices()
        {
            // Handle consolidated claims - split values by semicolon
            return _currentUser.Claims
                .Where(x => x.Type == EntityRoles.Device.ToString())
                .SelectMany(c => c.Value.Split(';', StringSplitOptions.RemoveEmptyEntries))
                .Select(v => Guid.TryParse(v, out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .Distinct()
                .ToList();
        }

        public List<Guid> GetLocations()
        {
            // Handle consolidated claims - split values by semicolon
            return _currentUser.Claims
                .Where(x => x.Type == EntityRoles.Location.ToString())
                .SelectMany(c => c.Value.Split(';', StringSplitOptions.RemoveEmptyEntries))
                .Select(v => Guid.TryParse(v, out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .Distinct()
                .ToList();
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
            return _mapper.Map<List<UserInfoDto>>(users);
        }

        public async Task<UserInfoDto?> GetUser(string userId)
        {
            if (!IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can view user details");
            }

            var user = await _userAuthService.GetUser(userId);
            return user == null ? null : _mapper.Map<UserInfoDto>(user);
        }

        public async Task ManageUserClaims(ManageUserClaimsRequest request)
        {
            if (!IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can manage user claims");
            }

            var claimsToAdd = request.ClaimsToAdd?.Select(c => new Claim(c.Type, c.Value)).ToList();
            var claimsToRemove = request.ClaimsToRemove?.Select(c => new Claim(c.Type, c.Value)).ToList();

            await _userAuthService.ManageUserClaims(request.UserId, claimsToAdd, claimsToRemove);
        }

        public async Task ManageUserRoles(ManageUserRolesRequest request)
        {
            if (!IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can manage user roles");
            }

            await _userAuthService.ManageUserRoles(request.UserId, request.RolesToAdd, request.RolesToRemove);
        }

        public AuthInfoCookie? GetAuthInfo()
        {
            return _userCookieService.ReadAuthInfoCookie();
        }

        public void ClearAuthInfo()
        {
            _userCookieService.ClearAuthInfoCookie();
        }

        public List<string> Roles => _currentUser.Roles.ToList();

    }
}