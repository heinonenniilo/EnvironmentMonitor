using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IUserCookieService
    {
        void WriteAuthFailureCookie(List<string> errors, string? errorCode = null, string? loginProvider = null);
        void WriteAuthSuccessCookie(string? loginProvider = null);
        AuthInfoCookie? ReadAuthInfoCookie();
        void ClearAuthInfoCookie();
    }
}
