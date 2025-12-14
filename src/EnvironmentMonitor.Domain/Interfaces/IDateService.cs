using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IDateService
    {
        public DateTime CurrentTime();
        public DateTime UtcToLocal(DateTime utc);
        public DateTime LocalToUtc(DateTime local);
        public TimeZoneInfo GetLocalTimeZone();
        public string FormatDateTime(DateTime dateTime);
    }
}
