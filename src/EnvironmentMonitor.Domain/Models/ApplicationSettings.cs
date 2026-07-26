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
        /// Time zone ID. Default: "FLE Standard Time" (Windows). 
        /// For Linux/macOS, use "Europe/Helsinki".
        /// </summary>
        public string TimeZone { get; set; } = "FLE Standard Time";
    }
}
