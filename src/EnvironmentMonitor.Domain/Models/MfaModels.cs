using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    /// <summary>
    /// Describes which multi factor authentication methods the user has enabled.
    /// </summary>
    public class MfaStatusModel
    {
        public bool TwoFactorEnabled { get; set; }
        public bool EmailEnabled { get; set; }
        public bool AuthenticatorEnabled { get; set; }
        public List<MfaMethods> EnabledMethods { get; set; } = new List<MfaMethods>();
    }

    /// <summary>
    /// Result of starting the setup of a multi factor authentication method.
    /// For the authenticator app the shared key and the otpauth uri are returned,
    /// for email the code is sent to the user's email address.
    /// </summary>
    public class MfaSetupResult
    {
        public MfaMethods Method { get; set; }
        public string? SharedKey { get; set; }
        public string? AuthenticatorUri { get; set; }
        public bool CodeSent { get; set; }
    }

    public class MfaConfirmModel
    {
        public MfaMethods Method { get; set; }
        public required string Code { get; set; }
    }

    public class MfaDisableModel
    {
        public MfaMethods Method { get; set; }
        public string? Code { get; set; }
    }

    /// <summary>
    /// Describes a pending two factor challenge for a user who has passed the first login step.
    /// </summary>
    public class MfaChallengeModel
    {
        public bool ChallengePending { get; set; }
        public List<MfaMethods> AvailableMethods { get; set; } = new List<MfaMethods>();
        public string? LoginProvider { get; set; }
        public string? MaskedEmail { get; set; }
    }

    public class MfaLoginModel
    {
        public MfaMethods Method { get; set; }
        public required string Code { get; set; }
        public bool Persistent { get; set; }
    }

    /// <summary>
    /// Result of a login attempt. When <see cref="RequiresTwoFactor"/> is true the caller
    /// must complete the login by posting a code from one of the available methods.
    /// </summary>
    public class LoginResultModel
    {
        public bool Success { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public List<MfaMethods> AvailableMethods { get; set; } = new List<MfaMethods>();
    }
}
