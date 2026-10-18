using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IUserAuthService
    {
        public Task<LoginResultModel> Login(LoginModel model);
        public Task Logout();
        public Task<ExternalLoginResult> LoginWithExternalProvider(ExternalLoginModel model);
        public Task RegisterUser(RegisterUserModel model);
        public Task<bool> ConfirmEmail(string userId, string token);
        public Task ChangePassword(string userId, ChangePasswordModel model);
        public Task ForgotPassword(ForgotPasswordModel model);
        public Task<bool> ResetPassword(ResetPasswordModel model);
        public Task DeleteUser(string userId);
        public Task<List<UserInfoModel>> GetAllUsers();
        public Task<UserInfoModel?> GetUser(string userId);
        public Task ManageUserClaims(string userId, List<Claim>? claimsToAdd, List<Claim>? claimsToRemove);
        public Task ManageUserRoles(string userId, List<string>? rolesToAdd, List<string>? rolesToRemove);

        // Multi factor authentication
        public Task<MfaStatusModel> GetMfaStatus(string userId);
        public Task<MfaSetupResult> StartMfaSetup(string userId, Domain.Enums.MfaMethods method);
        public Task<MfaStatusModel> ConfirmMfaSetup(string userId, MfaConfirmModel model);
        public Task<MfaStatusModel> DisableMfaMethod(string userId, MfaDisableModel model);

        // Pending two factor challenge (after the first login step)
        public Task<MfaChallengeModel> GetPendingMfaChallenge();
        public Task SendPendingMfaEmailCode();
        public Task<LoginResultModel> CompleteTwoFactorLogin(MfaLoginModel model);
    }
}
