using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class ManageUserRolesRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<string>? RolesToAdd { get; set; }
        public List<string>? RolesToRemove { get; set; }
    }
}
