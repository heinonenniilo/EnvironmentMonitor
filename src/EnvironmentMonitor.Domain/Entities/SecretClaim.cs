using System;

namespace EnvironmentMonitor.Domain.Entities
{
    public class SecretClaim
    {
        public int Id { get; set; }
        public required string Value { get; set; } = string.Empty;
        public required string Type { get; set; } = string.Empty;
        public required string ApiSecretId { get; set; } = string.Empty;

        public ApiSecret ApiSecret { get; set; } = null!;
    }
}
