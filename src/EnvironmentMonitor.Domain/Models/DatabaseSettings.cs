using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class DatabaseSettings
    {
        public const string SqlServer = "SqlServer";
        public const string PostgreSql = "PostgreSql";

        public string Provider { get; set; } = SqlServer;

        public bool IsMigration { get; set; } = false;
    }
}
