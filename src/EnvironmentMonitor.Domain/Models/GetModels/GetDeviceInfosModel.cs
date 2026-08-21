using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Models.GetModels
{
    public class GetDeviceInfosModel
    {
        public List<Guid>? Identifiers { get; set; }
        public List<Guid>? LocationIdentifiers { get; set; }
        public List<int>? CommunicationChannelIds { get; set; }
        public bool OnlyVisible { get; set; }
        public bool GetAttachments { get; set; }
        public bool GetLocation { get; set; }
        public bool GetAttributes { get; set; }
        public bool GetContacts { get; set; }
        public bool? IsVirtual { get; set; }
        public bool GetLatestMeasurementBySensor { get; set; }
    }
}
