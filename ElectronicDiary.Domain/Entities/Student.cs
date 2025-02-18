using ElectronicDiary.Domain.Common.Concretes;

namespace ElectronicDiary.Domain.Entities
{
    public class Student : BaseEntity<int>
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Grade>? Grades { get; set; }
        public ICollection<StudentAssignment>? StudentAssignments { get; set; }
        public ICollection<TeacherStudent>? TeacherStudents { get; set; }
    }
}