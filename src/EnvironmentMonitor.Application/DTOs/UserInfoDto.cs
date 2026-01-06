using AutoMapper;
using EnvironmentMonitor.Application.Mappings;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class UserInfoDto : IMapFrom<UserInfoModel>
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<UserClaimDto> Claims { get; set; } = new();
        public List<ExternalLoginInfoDto> ExternalLogins { get; set; } = new();
        public DateTime? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? Updated { get; set; }
        public string? UpdatedById { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UserInfoModel, UserInfoDto>();
        }
    }

    public class UserClaimDto : IMapFrom<Claim>
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Claim, UserClaimDto>();
        }
    }

    public class ExternalLoginInfoDto : IMapFrom<ExternalLoginInfoModel>
    {
        public string LoginProvider { get; set; } = string.Empty;
        public string? ProviderDisplayName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ExternalLoginInfoModel, ExternalLoginInfoDto>();
        }
    }
}
