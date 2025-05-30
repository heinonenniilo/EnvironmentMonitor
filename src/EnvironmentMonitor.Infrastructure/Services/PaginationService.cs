using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class PaginationService : IPaginationService
    {
        private readonly ILogger<PaginationService> _logger;
        public PaginationService(ILogger<PaginationService> logger)
        {
            _logger = logger;
        }
        public async Task<PaginatedResult<T>> PaginateAsync<T>(
            IQueryable<T> source,
            PaginationParams parameters,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking total count");
            var totalCount = await source.CountAsync(cancellationToken);
            _logger.LogInformation("Total coutn checked");

            if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
            {              
                var ordering = parameters.IsDescending
                    ? $"{parameters.OrderBy} descending"
                    : parameters.OrderBy;
                _logger.LogInformation($"Applying sort: {ordering}");
                source = source.OrderBy(ordering);
            }
           
            var items = await source
                .Skip(parameters.Skip)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
    }
}
