using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ElectronicDiary.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITeacherRepository> _teacherRepositoryMock;
        private readonly Mock<IStudentRepository> _studentRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthService _authService;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

        public AuthServiceTests()
        {
            _userManagerMock = GetMockUserManager();
            _signInManagerMock = GetMockSignInManager(_userManagerMock);
            _teacherRepositoryMock = new Mock<ITeacherRepository>();
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

            var jwtSectionMock = new Mock<IConfigurationSection>();
            jwtSectionMock.Setup(x => x["Key"]).Returns("TestKeyTestKeyTestKeyTestKeyTestKey");
            jwtSectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSectionMock.Setup(x => x["Audience"]).Returns("TestAudience");
            jwtSectionMock.Setup(x => x["ExpiresInMinutes"]).Returns("60");

            _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);
            _configurationMock.Setup(x => x["App:BaseUrl"]).Returns("http://localhost");

            _authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _teacherRepositoryMock.Object,
                _configurationMock.Object,
                _emailServiceMock.Object,
                _studentRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object
            );
        }

        #region Helper Methods

        private static Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            return mgr;
        }

        private static Mock<SignInManager<ApplicationUser>> GetMockSignInManager(Mock<UserManager<ApplicationUser>> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(
                userManager.Object,
                contextAccessor.Object,
                userClaimsPrincipalFactory.Object,
                null, null, null, null);
        }

        #endregion

        [Fact]
        public async Task RegisterTeacherAsync_Should_Register_Successfully()
        {
            // Arrange
            var userName = "teacher1";
            var email = "teacher1@example.com";
            var password = "Password123!";
            var firstName = "Teacher";
            var lastName = "One";
            var subject = "Math";
            var birthDate = new DateTime(1980, 1, 1);
            var userId = "user-id-123";
            var emailToken = "email-confirmation-token";

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, pass) => { user.Id = userId; });

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Teacher"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(emailToken);

            var emailTemplatePath = "../ElectronicDiary.Infrastructure/Services/EmailHtmlTemplates/ConfirmRegistEmailTemplate.html";
            var expectedTemplateContent = "Click here to confirm: {{confirmationLink}}";
            if (!File.Exists(emailTemplatePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(emailTemplatePath)!);
                await File.WriteAllTextAsync(emailTemplatePath, expectedTemplateContent);
            }

            // Act
            var result = await _authService.RegisterTeacherAsync(userName, email, password, firstName, lastName, subject, birthDate);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Empty(result.Errors);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Teacher"), Times.Once);
            _teacherRepositoryMock.Verify(x => x.AddAsync(It.Is<Teacher>(t =>
                t.ApplicationUserId == userId && t.Subject == subject &&
                t.FullName == $"{firstName} {lastName}")), Times.Once);
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailAsync(email, "Confirm your email",
                It.Is<string>(body => body.Contains("http://localhost/api/auth/confirm-email"))), Times.Once);
        }

        [Fact]
        public async Task RegisterStudentAsync_Should_Register_Successfully()
        {
            // Arrange
            var userName = "student1";
            var email = "student1@example.com";
            var password = "Password123!";
            var firstName = "Student";
            var lastName = "One";
            var birthDate = new DateTime(2000, 1, 1);
            var userId = "student-user-id";
            var emailToken = "email-confirmation-token";

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, pass) => { user.Id = userId; });

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(emailToken);

            var emailTemplatePath = "../ElectronicDiary.Infrastructure/Services/EmailHtmlTemplates/ConfirmRegistEmailTemplate.html";
            var expectedTemplateContent = "Click here to confirm: {{confirmationLink}}";
            if (!File.Exists(emailTemplatePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(emailTemplatePath)!);
                await File.WriteAllTextAsync(emailTemplatePath, expectedTemplateContent);
            }

            // Act
            var result = await _authService.RegisterStudentAsync(userName, email, password, firstName, lastName, birthDate);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Empty(result.Errors);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"), Times.Once);
            _studentRepositoryMock.Verify(x => x.AddAsync(It.Is<Student>(s =>
                s.ApplicationUserId == userId &&
                s.FullName == $"{firstName} {lastName}")), Times.Once);
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailAsync(email, "Confirm your email",
                It.Is<string>(body => body.Contains("http://localhost/api/auth/confirm-email"))), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Token_For_Valid_Credentials()
        {
            // Arrange
            var username = "user1";
            var password = "Password123!";
            var user = new ApplicationUser
            {
                Id = "user-id-456",
                UserName = username,
                Email = "user1@example.com",
                EmailConfirmed = true
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Teacher" });

            // Act
            var result = await _authService.LoginAsync(username, password);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(string.Empty, result.Error);
            Assert.NotNull(result.AccessToken);
            Assert.NotEmpty(result.AccessToken);
        }

        [Fact]
        public async Task LoginAsync_Should_Fail_For_Invalid_User()
        {
            // Arrange
            var username = "nonexistent";
            _userManagerMock.Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authService.LoginAsync(username, "anyPassword");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Invalid username or password", result.Error);
            Assert.Equal(string.Empty, result.AccessToken);
        }

        [Fact]
        public async Task LoginAsync_Should_Fail_If_Email_Not_Confirmed()
        {
            // Arrange
            var username = "user2";
            var user = new ApplicationUser
            {
                Id = "user-id-789",
                UserName = username,
                Email = "user2@example.com",
                EmailConfirmed = false
            };
            _userManagerMock.Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(username, "Password123!");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Email is not confirmed. Please check your inbox.", result.Error);
            Assert.Equal(string.Empty, result.AccessToken);
        }

        [Fact]
        public async Task ForgotPasswordAsync_Should_Send_Reset_Email_If_User_Found()
        {
            // Arrange
            var email = "user@example.com";
            var user = new ApplicationUser { Id = "user-id-101", Email = email };
            var resetToken = "reset-token-123";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(resetToken);

            var emailTemplatePath = "../ElectronicDiary.Infrastructure/Services/EmailHtmlTemplates/ResetPassEmailTemplate.html";
            var expectedTemplateContent = "Reset your password: {{confirmationLink}}";
            if (!File.Exists(emailTemplatePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(emailTemplatePath)!);
                await File.WriteAllTextAsync(emailTemplatePath, expectedTemplateContent);
            }

            // Act
            var result = await _authService.ForgotPasswordAsync(email);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(string.Empty, result.Error);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailAsync(email, "Password Reset",
                It.Is<string>(body => body.Contains("http://localhost/api/auth/reset-password"))), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_Should_Fail_If_User_Not_Found()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authService.ForgotPasswordAsync(email);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("User not found", result.Error);
        }

        [Fact]
        public async Task ResetPasswordAsync_Should_Reset_Password_Successfully()
        {
            // Arrange
            var email = "user@example.com";
            var user = new ApplicationUser { Id = "user-id-202", Email = email };
            var resetToken = "valid-reset-token";
            var newPassword = "NewPassword123!";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, resetToken, newPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ResetPasswordAsync(email, resetToken, newPassword);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Empty(result.Errors);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, resetToken, newPassword), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_Should_Fail_If_User_Not_Found()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authService.ResetPasswordAsync(email, "any-token", "NewPassword123!");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("User not found", result.Errors);
        }
    }
}
