using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.Pagination
{
    public class PaginationParams
    {
        public required int PageNumber { get; set; }
        public required int PageSize { get; set; }
        public string? OrderBy { get; set; }

        public bool IsDescending { get; set; }  

        public int Skip => (PageNumber - 1) * PageSize;
    }
}
