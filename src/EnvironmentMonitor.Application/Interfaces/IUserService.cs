using EnvironmentMonitor.Application.DTOs;
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
        public Task<ExternalLoginResult> ExternalLogin(ExternalLoginModel model);
        public Task RegisterUser(RegisterUserModel model);
        public Task<bool> ConfirmEmail(string userId, string token);
        public Task ChangePassword(ChangePasswordModel model);
        public Task ForgotPassword(ForgotPasswordModel model);
        public Task<bool> ResetPassword(ResetPasswordModel model);
        bool HasAccessTo(EntityRoles entity, Guid id, AccessLevels accessLevel);

        bool HasAccessToDevice(Guid id, AccessLevels accessLevel);
        bool HasAccessToSensor(Guid id, AccessLevels accessLevel);
        bool HasAccessToSensors(List<Guid> ids, AccessLevels accessLevel);
        bool HasAccessToDevices(List<Guid> ids, AccessLevels accessLevel);
        bool HasAccessToLocations(List<Guid> ids, AccessLevels accessLevel);
        public List<Guid> GetDevices();
        public List<Guid> GetLocations();
        public bool IsAdmin { get; }
        Task DeleteOwnUser();
        Task DeleteUser(string userId);
        Task<List<UserInfoDto>> GetAllUsers();
        Task<UserInfoDto?> GetUser(string userId);
        Task ManageUserClaims(ManageUserClaimsRequest request);
        Task ManageUserRoles(ManageUserRolesRequest request);
        AuthInfoCookie? GetAuthInfo();
        void ClearAuthInfo();
    }
}
