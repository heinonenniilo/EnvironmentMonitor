using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.GetModels
{
    public class GetLocationsModel
    {
        public List<int>? Ids { get; set; }
        public bool IncludeLocationSensors { get; set; }
        public bool? Visible { get; set; }
    }
}
