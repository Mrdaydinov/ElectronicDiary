using System.ComponentModel.DataAnnotations;

namespace ElectronicDiary.WebApi.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
