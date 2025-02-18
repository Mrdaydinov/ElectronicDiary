using System.ComponentModel.DataAnnotations;

namespace ElectronicDiary.WebApi.DTOs.Auth
{
    public class TeacherRegisterDto
    {
        [Required]
        public string Username { get; set; }
        [EmailAddress, Required]
        public string Email { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Subject { get; set; }
    }
}
