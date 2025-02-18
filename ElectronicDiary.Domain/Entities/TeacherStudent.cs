using ElectronicDiary.Domain.Common.Concretes;

namespace ElectronicDiary.Domain.Entities
{
    public class TeacherStudent : BaseEntity<int>
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
    }
}
