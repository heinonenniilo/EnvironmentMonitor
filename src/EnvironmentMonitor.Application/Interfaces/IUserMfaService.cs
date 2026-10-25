using EnvironmentMonitor.Application.DTOs;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IUserMfaService
    {
        // Requires an authenticated user
        Task<MfaStatusDto> GetMfaStatus();
        Task<MfaSetupDto> StartMfaSetup(MfaSetupRequest request);
        Task<MfaStatusDto> ConfirmMfaSetup(MfaConfirmRequest request);
        Task<MfaStatusDto> DisableMfaMethod(MfaDisableRequest request);

        // Pending two factor challenge (no authenticated user yet)
        Task<MfaChallengeDto> GetPendingMfaChallenge();
        Task SendPendingMfaEmailCode();
        Task<LoginResultDto> CompleteTwoFactorLogin(MfaLoginRequest request);
    }
}
