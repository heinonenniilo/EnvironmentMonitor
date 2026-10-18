using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class MfaStatusDto
    {
        public bool TwoFactorEnabled { get; set; }
        public bool EmailEnabled { get; set; }
        public bool AuthenticatorEnabled { get; set; }
        public List<MfaMethods> EnabledMethods { get; set; } = new List<MfaMethods>();
    }

    public class MfaSetupRequest
    {
        public MfaMethods Method { get; set; }
    }

    public class MfaSetupDto
    {
        public MfaMethods Method { get; set; }
        public string? SharedKey { get; set; }
        public string? AuthenticatorUri { get; set; }
        public bool CodeSent { get; set; }
    }

    public class MfaConfirmRequest
    {
        public MfaMethods Method { get; set; }
        public required string Code { get; set; }
    }

    public class MfaDisableRequest
    {
        public MfaMethods Method { get; set; }
        public string? Code { get; set; }
    }

    public class MfaChallengeDto
    {
        public bool ChallengePending { get; set; }
        public List<MfaMethods> AvailableMethods { get; set; } = new List<MfaMethods>();
        public string? LoginProvider { get; set; }
        public string? MaskedEmail { get; set; }
    }

    public class MfaLoginRequest
    {
        public MfaMethods Method { get; set; }
        public required string Code { get; set; }
        public bool Persistent { get; set; }
    }

    public class LoginResultDto
    {
        public bool Success { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public List<MfaMethods> AvailableMethods { get; set; } = new List<MfaMethods>();
        public string? Message { get; set; }
    }
}
