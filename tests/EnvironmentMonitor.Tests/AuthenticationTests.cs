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
        private const string TestUserEmail = "testuser@test.com";
        private const string TestUserPassword = "TestPassword#123";

        [Test]
        public async Task RegisterUser_ShouldCreateUserWithUnconfirmedEmail()
        {
            // Arrange & Act
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);

            // Assert
            Assert.That(success, Is.True, "User registration should succeed");
            Assert.That(userId, Is.Not.Empty, "User ID should be returned");
            
            var isConfirmed = await IsEmailConfirmedAsync(TestUserEmail);
            Assert.That(isConfirmed, Is.False, "Email should not be confirmed initially");
        }

        [Test]
        public async Task ConfirmEmail_WithValidToken_ShouldConfirmEmail()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);

            // Act
            var confirmSuccess = await ConfirmEmailAsync(userId, token);

            // Assert
            Assert.That(confirmSuccess, Is.True, "Email confirmation should succeed");
            
            var isConfirmed = await IsEmailConfirmedAsync(TestUserEmail);
            Assert.That(isConfirmed, Is.True, "Email should be confirmed");
        }

        [Test]
        public async Task ConfirmEmail_WithInvalidToken_ShouldFail()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var invalidToken = "invalid-token-12345";

            // Act
            var confirmSuccess = await ConfirmEmailAsync(userId, invalidToken);

            // Assert
            Assert.That(confirmSuccess, Is.False, "Email confirmation should fail with invalid token");
            
            var isConfirmed = await IsEmailConfirmedAsync(TestUserEmail);
            Assert.That(isConfirmed, Is.False, "Email should not be confirmed");
        }

        [Test]
        public async Task Login_WithUnconfirmedEmail_ShouldFail()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            // Act
            var loginSuccess = await LoginAsync(TestUserEmail, TestUserPassword);

            // Assert
            Assert.That(loginSuccess, Is.False, "Login should fail with unconfirmed email");
        }

        [Test]
        public async Task Login_WithConfirmedEmail_ShouldSucceed()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);

            // Act
            var loginSuccess = await LoginAsync(TestUserEmail, TestUserPassword);

            // Assert
            Assert.That(loginSuccess, Is.True, "Login should succeed with confirmed email");
        }

        [Test]
        public async Task ForgotPassword_WithValidEmail_ShouldSucceed()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var confirmToken = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, confirmToken);

            // Act
            var forgotPasswordData = new { Email = TestUserEmail };
            var content = new StringContent(JsonConvert.SerializeObject(forgotPasswordData), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Authentication/forgot-password", content);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True, "Forgot password request should succeed");
        }

        [Test]
        public async Task ResetPassword_WithValidToken_ShouldSucceed()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var confirmToken = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, confirmToken);

            var resetToken = await GeneratePasswordResetTokenAsync(TestUserEmail);
            var newPassword = "NewPassword#456";

            // Act
            var resetSuccess = await ResetPasswordAsync(TestUserEmail, resetToken, newPassword);

            // Assert
            Assert.That(resetSuccess, Is.True, "Password reset should succeed");

            // Verify can login with new password
            var loginSuccess = await LoginAsync(TestUserEmail, newPassword);
            Assert.That(loginSuccess, Is.True, "Should be able to login with new password");
        }

        [Test]
        public async Task ResetPassword_WithInvalidToken_ShouldFail()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var confirmToken = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, confirmToken);

            var invalidToken = "invalid-reset-token";
            var newPassword = "NewPassword#456";

            // Act
            var resetSuccess = await ResetPasswordAsync(TestUserEmail, invalidToken, newPassword);

            // Assert
            Assert.That(resetSuccess, Is.False, "Password reset should fail with invalid token");

            // Verify can still login with old password
            var loginSuccess = await LoginAsync(TestUserEmail, TestUserPassword);
            Assert.That(loginSuccess, Is.True, "Should still be able to login with old password");
        }

        [Test]
        public async Task ChangePassword_WhenAuthenticated_ShouldSucceed()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);

            var loginSuccess = await LoginAsync(TestUserEmail, TestUserPassword);
            Assert.That(loginSuccess, Is.True);

            var newPassword = "NewPassword#789";

            // Act
            var changeSuccess = await ChangePasswordAsync(TestUserPassword, newPassword);

            // Assert
            Assert.That(changeSuccess, Is.True, "Password change should succeed");

            // Logout and verify can login with new password
            await _client.PostAsync("/api/Authentication/logout", null);
            var loginWithNewPassword = await LoginAsync(TestUserEmail, newPassword);
            Assert.That(loginWithNewPassword, Is.True, "Should be able to login with new password");
        }

        [Test]
        public async Task ChangePassword_WhenNotAuthenticated_ShouldFail()
        {
            // Arrange
            var currentPassword = "OldPassword#123";
            var newPassword = "NewPassword#456";

            // Act
            var changeSuccess = await ChangePasswordAsync(currentPassword, newPassword);

            // Assert
            Assert.That(changeSuccess, Is.False, "Password change should fail when not authenticated");
        }

        [Test]
        public async Task ChangePassword_WithWrongCurrentPassword_ShouldFail()
        {
            // Arrange
            var (success, userId) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(success, Is.True);

            var token = await GenerateEmailConfirmationTokenAsync(userId);
            await ConfirmEmailAsync(userId, token);

            var loginSuccess = await LoginAsync(TestUserEmail, TestUserPassword);
            Assert.That(loginSuccess, Is.True);

            var wrongCurrentPassword = "WrongPassword#000";
            var newPassword = "NewPassword#789";

            // Act
            var changeSuccess = await ChangePasswordAsync(wrongCurrentPassword, newPassword);

            // Assert
            Assert.That(changeSuccess, Is.False, "Password change should fail with wrong current password");
        }

        [Test]
        public async Task RegisterUser_WithDuplicateEmail_ShouldFail()
        {
            // Arrange
            var (firstSuccess, _) = await RegisterUserAsync(TestUserEmail, TestUserPassword);
            Assert.That(firstSuccess, Is.True);

            // Act
            var (secondSuccess, _) = await RegisterUserAsync(TestUserEmail, TestUserPassword);

            // Assert
            Assert.That(secondSuccess, Is.False, "Should not be able to register with duplicate email");
        }
    }
}
