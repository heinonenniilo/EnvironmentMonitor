﻿using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IUserAuthService
    {
        public Task Login(LoginModel model);
        public Task LoginWithExternalProvider(ExternalLoginModel model);
        public Task RegisterUser(RegisterUserModel model);
    }
}
