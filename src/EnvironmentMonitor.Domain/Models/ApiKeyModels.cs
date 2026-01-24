using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Models
{
    public class CreateApiKeyRequest
    {
        public List<Guid> DeviceIds { get; set; } = new();
        public List<Guid> LocationIds { get; set; } = new();
        public string? Description { get; set; }
    }

    public class CreateApiKeyResponse
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Created { get; set; }
    }

    public class CreateApiSecretResult
    {
        public required ApiSecret ApiSecret { get; set; }
        public required string PlainKey { get; set; }
    }
}
