using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
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
        private readonly ApplicationSettings _applicationSettings;

        private TimeZoneInfo? _localTimeZone { get; set; }

        public DateService(ILogger<DateService> logger, ApplicationSettings applicationSettings)
        {
            _logger = logger;
            _applicationSettings = applicationSettings;
            Init();
        }

        public DateTime CurrentTime()
        {
            var dateToReturn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetLocalTimeZone());
            return DateTime.SpecifyKind(dateToReturn, DateTimeKind.Unspecified);
        }

        public TimeZoneInfo GetLocalTimeZone()
        {
            if (_localTimeZone == null)
            {
                _logger.LogError($"No valid time zone found or not inited. Zones: [{string.Join(", ", _applicationSettings.TimeZones)}]");
                throw new InvalidOperationException($"No valid time zone found or not inited");
            }
            return _localTimeZone;
        }

        public DateTime LocalToUtc(DateTime local)
        {
            var toReturn = TimeZoneInfo.ConvertTimeToUtc(local, GetLocalTimeZone());
            return DateTime.SpecifyKind(toReturn, DateTimeKind.Utc);
        }
        public DateTime UtcToLocal(DateTime utc)
        {
            var toReturn = TimeZoneInfo.ConvertTimeFromUtc(utc, GetLocalTimeZone());
            return DateTime.SpecifyKind(toReturn, DateTimeKind.Unspecified);
        }

        public string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private void Init()
        {
            _logger.LogInformation("Initializing DateService with time zones: [{TimeZones}]", string.Join(", ", _applicationSettings.TimeZones));

            foreach (var timeZoneId in _applicationSettings.TimeZones)
            {
                if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo? timeZone))
                {
                    _logger.LogInformation($"Time zone '{timeZoneId}' found: {timeZone.DisplayName}");
                    _localTimeZone = timeZone;
                    return;
                }
                else
                {
                    _logger.LogInformation($"Time zone '{timeZoneId}' not found, trying next");
                }
            }

            _logger.LogError($"No valid time zone found from configured options: [{string.Join(", ", _applicationSettings.TimeZones)}]");
            _localTimeZone = null;
        }
    }
}
