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

        public DateService(ILogger<DateService> logger, ApplicationSettings applicationSettings)
        {
            _logger = logger;
            _applicationSettings = applicationSettings;
        }

        public DateTime CurrentTime() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetLocalTimeZone());

        public TimeZoneInfo GetLocalTimeZone()
        {
            return TimeZoneInfo.FindSystemTimeZoneById(_applicationSettings.TimeZone);
        }

        public DateTime LocalToUtc(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(local, GetLocalTimeZone());
        public DateTime UtcToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, GetLocalTimeZone());

        public string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
        }
    }
}
