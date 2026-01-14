using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Text;
using Assert = NUnit.Framework.Assert;

namespace EnvironmentMonitor.Tests
{
    [TestFixture]
    public class UserManagementTests : BaseIntegrationTest
    {
        private const string TestUserEmail = "testuser@test.com";
        private const string TestUserPassword = "TestUser#123";

        [Test]
        public async Task NonAdminUserCannotAccessUserManagementEndpoints()
        {
            // Arrange
            await PrepareDatabase();
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);
            await LoginAsync(TestUserEmail, TestUserPassword);

            // Act
            var getAllResponse = await _client.GetAsync("/api/usermanagement");
            var getUserResponse = await _client.GetAsync($"/api/usermanagement/{userId}");
            var deleteResponse = await _client.DeleteAsync($"/api/usermanagement/{userId}");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(getUserResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            });
        }

        [Test]
        public async Task AdminCanAccessUserManagementEndpoints()
        {
            // Arrange
            await PrepareDatabase();
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);
            await LoginAsync(AdminUserName, AdminPassword);

            // Act
            var getAllResponse = await _client.GetAsync("/api/usermanagement");
            var getUserResponse = await _client.GetAsync($"/api/usermanagement/{userId}");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(getUserResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            });
        }

        [Test]
        public async Task ManageUserClaims_RequiresAdminRole()
        {
            // Arrange
            await PrepareDatabase();
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);
            await LoginAsync(ViewerUserName, ViewerPassword);

            var request = new ManageUserClaimsRequest
            {
                UserId = userId,
                ClaimsToAdd = new List<UserClaimDto>
                {
                    new UserClaimDto { Type = "TestClaim", Value = "TestValue" }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/usermanagement/claims", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ManageUserRoles_RequiresAdminRole()
        {
            // Arrange
            await PrepareDatabase();
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);
            await LoginAsync(ViewerUserName, ViewerPassword);

            var request = new ManageUserRolesRequest
            {
                UserId = userId,
                RolesToAdd = new List<string> { GlobalRoles.User.ToString() }
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/usermanagement/roles", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task AdminCanAddClaimsToUser()
        {
            // Arrange
            await PrepareDatabase();
            var (success, targetUserId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(targetUserId);
            await ConfirmEmailAsync(targetUserId, token);
            await LoginAsync(AdminUserName, AdminPassword);

            var testClaimType = "TestClaimType";
            var testClaimValue = "TestClaimValue";

            var request = new ManageUserClaimsRequest
            {
                UserId = targetUserId,
                ClaimsToAdd = new List<UserClaimDto>
                {
                    new UserClaimDto { Type = testClaimType, Value = testClaimValue }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/usermanagement/claims", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Verify claim was added in database
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(targetUserId);
                var claims = await userManager.GetClaimsAsync(user!);
                
                Assert.That(claims.Any(c => c.Type == testClaimType && c.Value == testClaimValue), Is.True);
            }
        }

        [Test]
        public async Task AdminCanAddRolesToUser()
        {
            // Arrange
            await PrepareDatabase();
            var (success, targetUserId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(targetUserId);
            await ConfirmEmailAsync(targetUserId, token);
            await LoginAsync(AdminUserName, AdminPassword);

            var request = new ManageUserRolesRequest
            {
                UserId = targetUserId,
                RolesToAdd = new List<string> { GlobalRoles.User.ToString() }
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/usermanagement/roles", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Verify role was added in database
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(targetUserId);
                var roles = await userManager.GetRolesAsync(user!);
                
                Assert.That(roles.Contains(GlobalRoles.User.ToString()), Is.True);
            }
        }

        [Test]
        public async Task DeletingUser_ClearsUpdatedByIdReferences()
        {
            // Arrange
            await PrepareDatabase();
            // Create a target user that will be deleted
            var (targetSuccess, targetUserId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(targetSuccess, Is.True);
            var targetToken = await GenerateEmailConfirmationTokenAsync(targetUserId);
            await ConfirmEmailAsync(targetUserId, targetToken);
            // Create another user that will be modified
            var (otherSuccess, otherUserId) = await RegisterUserAsync("other@test.com", TestUserPassword);
            Assert.That(otherSuccess, Is.True);
            var otherToken = await GenerateEmailConfirmationTokenAsync(otherUserId);
            await ConfirmEmailAsync(otherUserId, otherToken);
            // Give target user admin role
            await LoginAsync(AdminUserName, AdminPassword);
            var grantAdminRequest = new ManageUserRolesRequest
            {
                UserId = targetUserId,
                RolesToAdd = new List<string> { GlobalRoles.Admin.ToString() }
            };
            var grantAdminContent = new StringContent(JsonConvert.SerializeObject(grantAdminRequest), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/usermanagement/roles", grantAdminContent);
            // Target user adds claims to another user
            await LoginAsync(TestUserEmail, TestUserPassword);
            var addClaimRequest = new ManageUserClaimsRequest
            {
                UserId = otherUserId,
                ClaimsToAdd = new List<UserClaimDto>
                {
                    new UserClaimDto { Type = "ModifiedByTargetUser", Value = "TestValue" }
                }
            };
            var addClaimContent = new StringContent(JsonConvert.SerializeObject(addClaimRequest), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/usermanagement/claims", addClaimContent);
            // Act - Delete target user
            await LoginAsync(AdminUserName, AdminPassword);
            var deleteResponse = await _client.DeleteAsync($"/api/usermanagement/{targetUserId}");
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            // Assert - Verify UpdatedById references are cleared
            using (var scope = _factory.Services.CreateScope())
            {
                var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var adminUser = await applicationDbContext.Users
                    .FirstOrDefaultAsync(u => u.UserName == AdminUserName);

                var deletedUser = await applicationDbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == targetUserId);
                Assert.That(deletedUser, Is.Null);

                var otherUser = await applicationDbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == otherUserId);
                Assert.That(otherUser, Is.Not.Null);

                Assert.That(otherUser.UpdatedById, Is.EqualTo(adminUser.Id));

                var claimsWithTargetUserId = await applicationDbContext.UserClaims
                    .AsNoTracking()
                    .Where(c => c.UpdatedById == targetUserId)
                    .CountAsync();
                Assert.That(claimsWithTargetUserId, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task AdminCanDeleteUser()
        {
            // Arrange
            await PrepareDatabase();
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);
            await LoginAsync(AdminUserName, AdminPassword);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/usermanagement/{userId}");

            // Assert
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Verify user is deleted
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var deletedUser = await userManager.FindByIdAsync(userId);
                Assert.That(deletedUser, Is.Null);
            }
        }

        [Test]
        public async Task GetUser_ReturnsUserDetails()
        {
            // Arrange
            await PrepareDatabase();
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);
            await LoginAsync(AdminUserName, AdminPassword);

            // Act
            var response = await _client.GetAsync($"/api/usermanagement/{userId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetAllUsers_ReturnsListOfUsers()
        {
            // Arrange
            await PrepareDatabase();
            var (success1, userId1) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            var (success2, userId2) = await RegisterUserAsync("another@test.com", TestUserPassword);
            Assert.That(success1 && success2, Is.True);

            var token1 = await GenerateEmailConfirmationTokenAsync(userId1);
            var token2 = await GenerateEmailConfirmationTokenAsync(userId2);
            await ConfirmEmailAsync(userId1, token1);
            await ConfirmEmailAsync(userId2, token2);

            await LoginAsync(AdminUserName, AdminPassword);

            // Act
            var response = await _client.GetAsync("/api/usermanagement");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task RemoveUserClaims_WorksCorrectly()
        {
            // Arrange
            await PrepareDatabase();
            var (success, targetUserId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(targetUserId);
            await ConfirmEmailAsync(targetUserId, token);
            await LoginAsync(AdminUserName, AdminPassword);

            var testClaimType = "TestClaim";
            var testClaimValue = "TestValue";

            // First add a claim
            var addRequest = new ManageUserClaimsRequest
            {
                UserId = targetUserId,
                ClaimsToAdd = new List<UserClaimDto>
                {
                    new UserClaimDto { Type = testClaimType, Value = testClaimValue }
                }
            };

            var addContent = new StringContent(JsonConvert.SerializeObject(addRequest), Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("/api/usermanagement/claims", addContent);
            Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Now remove the claim
            var removeRequest = new ManageUserClaimsRequest
            {
                UserId = targetUserId,
                ClaimsToRemove = new List<UserClaimDto>
                {
                    new UserClaimDto { Type = testClaimType, Value = testClaimValue }
                }
            };

            var removeContent = new StringContent(JsonConvert.SerializeObject(removeRequest), Encoding.UTF8, "application/json");

            // Act
            var removeResponse = await _client.PostAsync("/api/usermanagement/claims", removeContent);

            // Assert
            Assert.That(removeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Verify claim was removed in database
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(targetUserId);
                var claims = await userManager.GetClaimsAsync(user!);
                
                Assert.That(claims.Any(c => c.Type == testClaimType && c.Value == testClaimValue), Is.False);
            }
        }

        [Test]
        public async Task RemoveUserRoles_WorksCorrectly()
        {
            // Arrange
            await PrepareDatabase();
            var (success, targetUserId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(targetUserId);
            await ConfirmEmailAsync(targetUserId, token);
            await LoginAsync(AdminUserName, AdminPassword);

            var roleToAdd = GlobalRoles.User.ToString();

            // First add a role
            var addRequest = new ManageUserRolesRequest
            {
                UserId = targetUserId,
                RolesToAdd = new List<string> { roleToAdd }
            };

            var addContent = new StringContent(JsonConvert.SerializeObject(addRequest), Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("/api/usermanagement/roles", addContent);
            Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Now remove the role
            var removeRequest = new ManageUserRolesRequest
            {
                UserId = targetUserId,
                RolesToRemove = new List<string> { roleToAdd }
            };

            var removeContent = new StringContent(JsonConvert.SerializeObject(removeRequest), Encoding.UTF8, "application/json");

            // Act
            var removeResponse = await _client.PostAsync("/api/usermanagement/roles", removeContent);

            // Assert
            Assert.That(removeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Verify role was removed in database
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(targetUserId);
                var roles = await userManager.GetRolesAsync(user!);
                
                Assert.That(roles.Contains(roleToAdd), Is.False);
            }
        }
    }
}
