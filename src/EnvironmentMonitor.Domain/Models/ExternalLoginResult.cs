using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Models
{
    public class ExternalLoginResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? ErrorCode { get; set; }
        public string? LoginProvider { get; set; }

        /// <summary>
        /// True when the first login step succeeded but a two factor code is still required.
        /// </summary>
        public bool RequiresTwoFactor { get; set; }
        public List<MfaMethods> AvailableMethods { get; set; } = new List<MfaMethods>();
    }
}
