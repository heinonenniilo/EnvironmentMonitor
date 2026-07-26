using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class ApplicationSettings
    {
        public string BaseUrl { get; set; } = "";
        public bool IsProduction { get; set; } = true;

        /// <summary>
        /// Time zone IDs to try in order. First available time zone will be used.
        /// Example: ["FLE Standard Time", "Europe/Helsinki"] for Windows/Linux compatibility.
        /// </summary>
        public string[] TimeZones { get; set; } = ["FLE Standard Time", "Europe/Helsinki"];
    }
}
