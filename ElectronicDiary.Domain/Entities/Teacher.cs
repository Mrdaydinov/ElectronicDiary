using ElectronicDiary.Domain.Common.Concretes;

namespace ElectronicDiary.Domain.Entities
{
    public class Teacher : BaseEntity<int>
    {
        public string FullName { get; set; }
        public string Subject { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Assignment>? Assignments { get; set; }
        public ICollection<TeacherStudent>? TeacherStudents { get; set; }

    }
}
