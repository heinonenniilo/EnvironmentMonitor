using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IUserService
    {
        public Task Login(LoginModel model);
        public Task ExternalLogin(ExternalLoginModel model);
        public Task RegisterUser(RegisterUserModel model);
        bool HasAccessToDevice(int id, AccessLevels accessLevel);
        bool HasAccessToSensor(int id, AccessLevels accessLevel);
        bool HasAccessToSensors(List<int> ids, AccessLevels accessLevel);
        bool HasAccessToDevices(List<int> ids, AccessLevels accessLevel);
    }
}
