using EnvironmentMonitor.Domain.Models.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IPaginationService
    {
        Task<PaginatedResult<T>> PaginateAsync<T>(
            IQueryable<T> source,
            PaginationParams parameters,
            CancellationToken cancellationToken = default);
    }
}
