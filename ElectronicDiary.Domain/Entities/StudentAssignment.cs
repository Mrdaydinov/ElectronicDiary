using ElectronicDiary.Domain.Common.Concretes;

namespace ElectronicDiary.Domain.Entities
{
    public class StudentAssignment : BaseEntity<int>
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
    }
}
