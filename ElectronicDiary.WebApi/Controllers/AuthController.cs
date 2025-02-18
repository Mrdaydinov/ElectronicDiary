using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.WebApi.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicDiary.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _logger = logger;
            _userManager = userManager;
        }


        [HttpPost("teacher-register")]
        public async Task<IActionResult> TeacherRegister([FromBody] TeacherRegisterDto dto)
        {
            _logger.LogInformation("User {UserId} is registering", dto.Username);

            var result = await _authService.RegisterTeacherAsync(dto.Username, dto.Email, dto.Password, dto.FirstName, dto.LastName, dto.Subject, dto.BirthDate);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration error for user {Username}: {Errors}", dto.Username, result.Errors);
                return BadRequest(new { result.Errors });
            }

            _logger.LogInformation("User {UserName} registered successfully", dto.Username);
            return Ok(new { Message = "Teacher registered successfully, confirm your e-mail" });
        }

        [HttpPost("student-register")]
        public async Task<IActionResult> StudentRegister([FromBody] StudentRegisterDto dto)
        {
            _logger.LogInformation("User {UserId} is registering", dto.Username);

            var result = await _authService.RegisterStudentAsync(dto.Username, dto.Email, dto.Password, dto.FirstName, dto.LastName, dto.BirthDate);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration error for user {Username}: {Errors}", dto.Username, result.Errors);
                return BadRequest(new { result.Errors });
            }

            _logger.LogInformation("User {UserName} registered successfully", dto.Username);
            return Ok(new { Message = "Student registered successfully, confirm your e-mail" });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Invalid user");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("Email confirmed successfully!");
            }
            return BadRequest("Invalid token");
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(new { message = "Password reset link sent." });
        }

        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            _logger.LogInformation("User {Username} is attempting to log in", dto.Username);

            var result = await _authService.LoginAsync(dto.Username, dto.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", dto.Username);
                return Unauthorized(new { Message = result.Error });
            }

            _logger.LogInformation("User {Username} successfully logged in", dto.Username);
            return Ok(new
            {
                result.AccessToken,
                result.RefreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken
            });
        }
    }
}
