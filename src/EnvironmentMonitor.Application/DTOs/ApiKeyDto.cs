using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Application.DTOs
{
    public class ApiKeyDto : IMapFrom<ApiSecret>
    {
        public string Id { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Created { get; set; }
        public List<ApiKeyClaimDto> Claims { get; set; } = new();

        public DateTime? Updated { get; set; }

        public bool Enabled { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ApiSecret, ApiKeyDto>();
        }
    }

    public class ApiKeyClaimDto : IMapFrom<SecretClaim>
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<SecretClaim, ApiKeyClaimDto>();
        }
    }
}
