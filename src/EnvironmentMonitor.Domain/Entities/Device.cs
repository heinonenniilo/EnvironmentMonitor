﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class Device
    {
        public int Id { get; set; }
        public required string DeviceIdentifier { get; set; }
        public string Name { get; set; }
        public ICollection<Sensor> Sensors { get; set; }
        public ICollection<DeviceEvent> Events { get; set; }
        public bool Visible { get; set; }
        public int? TypeId { get; set; }
        public DeviceType? Type { get; set; }
        public bool HasMotionSensor { get; set; }
    }
}
