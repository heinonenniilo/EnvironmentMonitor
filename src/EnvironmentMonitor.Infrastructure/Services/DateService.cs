using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class DateService : IDateService
    {
        private readonly ILogger<DateService> _logger;
        public DateService(ILogger<DateService> logger)
        {
            _logger = logger;
        }
        public DateTime CurrentTime() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetLocalTimeZone());

        public TimeZoneInfo GetLocalTimeZone()
        {
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ApplicationConstants.TargetTimeZone);
            if (targetTimeZone == null)
            {
                _logger.LogError($"Could not find time zone '{ApplicationConstants.TargetTimeZone}'");
                throw new InvalidOperationException("Local time zone not found");
            }
            return targetTimeZone;
        }

        public DateTime LocalToUtc(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(local, GetLocalTimeZone());
        public DateTime UtcToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, GetLocalTimeZone());
    }
}
