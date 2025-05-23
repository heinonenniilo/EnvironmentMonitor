﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class Attachment: TrackedEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Extension { get; set; }
        public string? OriginalName { get; set; }
        public string? ContentType { get; set; }
        public required string Path { get; set; }
        public string? FullPath { get; set; }
        public bool IsImage { get; set; } = true;

        public List<DeviceAttachment> DeviceAttachments { get; set; }
    }
}
