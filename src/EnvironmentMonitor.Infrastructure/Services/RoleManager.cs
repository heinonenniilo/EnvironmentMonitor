using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Extensions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class RoleManager : IRoleManager
    {
        private readonly RoleManager<ApplicationUserRole> _roleManager;
        private readonly ILogger<RoleManager> _logger;

        public RoleManager(ILogger<RoleManager> logger, RoleManager<ApplicationUserRole> roleManager)
        {
            _roleManager = roleManager;
            _logger = logger;
        }
        public async Task SetRoles()
        {
            foreach (var roleName in Enum.GetValues(typeof(GlobalRoles)).Cast<GlobalRoles>())
            {
                if (!await _roleManager.RoleExistsAsync(roleName.ToString()))
                {
                    var result = await _roleManager.CreateAsync(new ApplicationUserRole
                    {
                        Name = roleName.ToString(),
                        Description = roleName.GetDescription()
                    });

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create role '{roleName}':");
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"- {error.Description}");
                        }
                    }
                }
            }
        }
    }
}
