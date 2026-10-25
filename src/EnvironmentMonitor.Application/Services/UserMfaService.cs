using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class UserMfaService : IUserMfaService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserAuthService _userAuthService;
        private readonly IUserCookieService _userCookieService;

        public UserMfaService(
            ICurrentUser currentUser,
            IUserAuthService userAuthService,
            IUserCookieService userCookieService)
        {
            _currentUser = currentUser;
            _userAuthService = userAuthService;
            _userCookieService = userCookieService;
        }

        public async Task<MfaStatusDto> GetMfaStatus()
        {
            var status = await _userAuthService.GetMfaStatus(_currentUser.GetCurrentUserId());
            return ToDto(status);
        }

        public async Task<MfaSetupDto> StartMfaSetup(MfaSetupRequest request)
        {
            var result = await _userAuthService.StartMfaSetup(_currentUser.GetCurrentUserId(), request.Method);
            return new MfaSetupDto
            {
                Method = result.Method,
                SharedKey = result.SharedKey,
                AuthenticatorUri = result.AuthenticatorUri,
                CodeSent = result.CodeSent
            };
        }

        public async Task<MfaStatusDto> ConfirmMfaSetup(MfaConfirmRequest request)
        {
            var status = await _userAuthService.ConfirmMfaSetup(_currentUser.GetCurrentUserId(), new MfaConfirmModel
            {
                Method = request.Method,
                Code = request.Code
            });
            return ToDto(status);
        }

        public async Task<MfaStatusDto> DisableMfaMethod(MfaDisableRequest request)
        {
            var status = await _userAuthService.DisableMfaMethod(_currentUser.GetCurrentUserId(), new MfaDisableModel
            {
                Method = request.Method,
                Code = request.Code
            });
            return ToDto(status);
        }

        public async Task<MfaChallengeDto> GetPendingMfaChallenge()
        {
            var challenge = await _userAuthService.GetPendingMfaChallenge();
            return new MfaChallengeDto
            {
                ChallengePending = challenge.ChallengePending,
                AvailableMethods = challenge.AvailableMethods,
                LoginProvider = challenge.LoginProvider,
                MaskedEmail = challenge.MaskedEmail
            };
        }

        public async Task SendPendingMfaEmailCode()
        {
            await _userAuthService.SendPendingMfaEmailCode();
        }

        public async Task<LoginResultDto> CompleteTwoFactorLogin(MfaLoginRequest request)
        {
            var challenge = await _userAuthService.GetPendingMfaChallenge();
            var result = await _userAuthService.CompleteTwoFactorLogin(new MfaLoginModel
            {
                Method = request.Method,
                Code = request.Code,
                Persistent = request.Persistent
            });

            _userCookieService.WriteAuthSuccessCookie(string.IsNullOrEmpty(challenge.LoginProvider) ? "Local" : challenge.LoginProvider);
            return new LoginResultDto { Success = result.Success, Message = "Login successful!" };
        }

        private static MfaStatusDto ToDto(MfaStatusModel model) => new MfaStatusDto
        {
            EnabledMethods = model.EnabledMethods
        };
    }
}
