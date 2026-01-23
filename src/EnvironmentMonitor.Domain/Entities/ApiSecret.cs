using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Entities
{
    public class ApiSecret : TrackedEntity
    {
        public required string Id { get; set; }
        public byte[] Hash { get; set; } = Array.Empty<byte>();
        public string? Description { get; set; }
        public bool Enabled { get; set; } = true;
        public ICollection<SecretClaim> Claims { get; set; } = new List<SecretClaim>();
    }
}
