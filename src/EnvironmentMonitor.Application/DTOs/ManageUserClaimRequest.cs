using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class ManageUserClaimsRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<UserClaimDto>? ClaimsToAdd { get; set; }
        public List<UserClaimDto>? ClaimsToRemove { get; set; }
    }
}
