using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.GetModels
{
    public class Paginated
    {
        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 100;
        public string? OrderBy { get; set; }
        public bool IsDescending { get; set; }
    }
}
