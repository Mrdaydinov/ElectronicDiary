using Microsoft.AspNetCore.Identity;

namespace ElectronicDiary.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime BirthDate { get; set; }
    }
}
