using System.Globalization;

namespace ElectronicDiary.Application.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterTeacherAsync(string userName, string email, string password, string firstName, string lastName, string subject, DateTime birthDate);
        Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterStudentAsync(string userName, string email, string password, string firstName, string lastName, DateTime birthDate);
        Task<(bool Succeeded, string Error, string AccessToken, string RefreshToken)> LoginAsync(string username, string password);
        Task<(bool Succeeded, string Error)> ForgotPasswordAsync(string email);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword);
        Task<(bool Succeeded, string Error, string AccessToken, string RefreshToken)> RefreshTokenAsync(string token);
    }
}