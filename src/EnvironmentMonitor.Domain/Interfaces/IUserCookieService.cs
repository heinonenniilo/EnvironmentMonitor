using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IUserCookieService
    {
        void WriteAuthFailureCookie(List<string> errors, string? errorCode = null);
        void WriteAuthSuccessCookie();
        AuthInfoCookie? ReadAuthInfoCookie();
        void ClearAuthInfoCookie();
    }
}
