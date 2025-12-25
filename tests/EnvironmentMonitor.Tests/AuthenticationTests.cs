using NUnit.Framework;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using Assert = NUnit.Framework.Assert;

namespace EnvironmentMonitor.Tests
{
    [TestFixture]
    public class AuthenticationTests : BaseIntegrationTest
    {
        private string _testUserEmail;
        private const string TestUserPassword = "TestPassword#123";

        [SetUp]
        public void TestSetup()
        {
            // Use unique email for each test to avoid conflicts and enable parallel execution
            _testUserEmail = $"testuser_{Guid.NewGuid():N}@test.com";
        }

        [Test]
        public async Task RegisterUser_ShouldCreateUserWithUnconfirmedEmail()
        {
            // Arrange & Act
            var (success, userId) = await RegisterUserAsync(_testUserEmail, TestUserPassword);

            // Assert
            Assert.That(success, Is.True, "User registration should succeed");
            Assert.That(userId, Is.Not.Empty, "User ID should be returned");
            
            var isConfirmed = await IsEmailConfirmedAsync(_testUserEmail);
            Assert.That(isConfirmed, Is.False, "Email should not be confirmed initially");
        }

        [Test]
        public async Task Login_WithConfirmedEmail_ShouldSucceed()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(_testUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);

            // Act
            var loginSuccess = await LoginAsync(_testUserEmail, TestUserPassword);

            // Assert
            Assert.That(loginSuccess, Is.True, "Login should succeed with confirmed email");
        }

        [Test]
        public async Task ResetPassword_WithValidToken_ShouldSucceed()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(_testUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var confirmToken = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, confirmToken);

            var resetToken = await GeneratePasswordResetTokenAsync(_testUserEmail);
            var newPassword = "NewPassword#456";

            // Act
            var resetSuccess = await ResetPasswordAsync(_testUserEmail, resetToken, newPassword);

            // Assert
            Assert.That(resetSuccess, Is.True, "Password reset should succeed");

            // Verify can login with new password
            var loginSuccess = await LoginAsync(_testUserEmail, newPassword);
            Assert.That(loginSuccess, Is.True, "Should be able to login with new password");
        }

        [Test]
        public async Task RegisterUser_WithDuplicateEmail_ShouldFail()
        {
            // Arrange
            var (firstSuccess, _) = await RegisterUserAsync(_testUserEmail, TestUserPassword);
            Assert.That(firstSuccess, Is.True);

            // Act
            var (secondSuccess, _) = await RegisterUserAsync(_testUserEmail, TestUserPassword);

            // Assert
            Assert.That(secondSuccess, Is.False, "Should not be able to register with duplicate email");
        }
    }
}
