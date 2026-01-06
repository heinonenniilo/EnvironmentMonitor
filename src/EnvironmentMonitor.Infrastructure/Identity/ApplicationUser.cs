using EnvironmentMonitor.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime? Updated { get; set; }
        public DateTime? UpdatedUtc { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public string? UpdatedById { get; set; }
        public ApplicationUser? UpdatedBy { get; set; }
    }
}
