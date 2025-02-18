using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicDiary.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IStudentRepository _studentRepository; 
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ITeacherRepository teacherRepository,
            IConfiguration configuration,
            IEmailService emailService,
            IStudentRepository studentRepository,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _teacherRepository = teacherRepository;
            _configuration = configuration;
            _emailService = emailService;
            _studentRepository = studentRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterTeacherAsync(string userName, string email, string password, string firstName, string lastName, string subject, DateTime birthDate)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, "Teacher");

            var teacher = new Teacher
            {
                ApplicationUserId = user.Id,
                FullName = $"{firstName} {lastName}",
                Subject = subject,
            };

            await _teacherRepository.AddAsync(teacher);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{_configuration["App:BaseUrl"]}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            //
            var emailTemplatePath = "../ElectronicDiary.Infrastructure/Services/EmailHtmlTemplates/ConfirmRegistEmailTemplate.html";
            var emailBody = await File.ReadAllTextAsync(emailTemplatePath);
            emailBody = emailBody.Replace("{{confirmationLink}}", confirmationLink);

            await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

            return (true, Enumerable.Empty<string>());
        }


        public async Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterStudentAsync(string userName, string email, string password, string firstName, string lastName, DateTime birthDate)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, "Student");

            var student = new Student
            {
                ApplicationUserId = user.Id,
                FullName = $"{firstName} {lastName}",
                DateOfBirth = birthDate
            };

            await _studentRepository.AddAsync(student);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{_configuration["App:BaseUrl"]}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            //
            var emailTemplatePath = "../ElectronicDiary.Infrastructure/Services/EmailHtmlTemplates/ConfirmRegistEmailTemplate.html";
            var emailBody = await File.ReadAllTextAsync(emailTemplatePath);
            emailBody = emailBody.Replace("{{confirmationLink}}", confirmationLink);

            await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

            return (true, Enumerable.Empty<string>());
        }




        public async Task<(bool Succeeded, string Error, string AccessToken, string RefreshToken)> LoginAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return (false, "Invalid username or password", string.Empty, string.Empty);
            }

            if (!user.EmailConfirmed)
            {
                return (false, "Email is not confirmed. Please check your inbox.", string.Empty, string.Empty);
            }

            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            if (!result.Succeeded)
            {
                return (false, "Invalid username or password", string.Empty, string.Empty);
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(user);
            await _refreshTokenRepository.AddAsync(refreshToken);

            return (true, string.Empty, accessToken, refreshToken.Token);
        }


        public async Task<(bool Succeeded, string Error, string AccessToken, string RefreshToken)> RefreshTokenAsync(string token)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(token);
            if (existingToken == null || !existingToken.IsActive)
            {
                return (false, "Invalid or expired refresh token", null, null);
            }

            var user = await _userManager.FindByIdAsync(existingToken.UserId);
            if (user == null)
            {
                return (false, "User not found", null, null);
            }

            existingToken.Revoked = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(existingToken);

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(user);
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            return (true, string.Empty, newAccessToken, newRefreshToken.Token);
        }


        public async Task<(bool Succeeded, string Error)> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, "User not found");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_configuration["App:BaseUrl"]}/api/auth/reset-password?email={email}&token={Uri.EscapeDataString(token)}&newPassword=stringG123";

            var emailTemplatePath = "../ElectronicDiary.Infrastructure/Services/EmailHtmlTemplates/ResetPassEmailTemplate.html";
            var emailBody = await File.ReadAllTextAsync(emailTemplatePath);
            emailBody = emailBody.Replace("{{confirmationLink}}", resetLink);

            await _emailService.SendEmailAsync(email, "Password Reset", emailBody);

            return (true, string.Empty);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, new List<string> { "User not found" });
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description));
            }

            return (true, Enumerable.Empty<string>());
        }


        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var userRole = _userManager.GetRolesAsync(user).Result.FirstOrDefault();

            var authClaims = new[]
            {
                  new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                  new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                  new Claim(ClaimTypes.Role, userRole)
            };


            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: authClaims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(ApplicationUser user)
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                var token = Convert.ToBase64String(randomBytes);
                return new RefreshToken
                {
                    Token = token,
                    UserId = user.Id,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
            }
        }
    }
}
