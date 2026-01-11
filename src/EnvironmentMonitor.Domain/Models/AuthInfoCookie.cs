using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Models
{
    public class AuthInfoCookie
    {
        public bool LoginState { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? ErrorCode { get; set; }
        public string? LoginProvider { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
