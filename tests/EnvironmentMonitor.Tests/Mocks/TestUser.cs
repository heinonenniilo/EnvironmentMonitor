using EnvironmentMonitor.Domain.Interfaces;
using System.Security.Claims;

namespace EnvironmentMonitor.Tests.Mocks
{
    /// <summary>
    /// Test implementation of ICurrentUser for testing purposes
    /// </summary>
    public class TestUser : ICurrentUser
    {
        public string? Id { get; set; }
        public List<Claim> Claims { get; set; } = [];

        public string Email => "test_user@tester.com";

        public List<string> Roles => ["Admin", "TEST"];
    }
}
