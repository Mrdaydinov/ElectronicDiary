using ElectronicDiary.Domain.Common.Concretes;

namespace ElectronicDiary.Domain.Entities
{
    public class Grade : BaseEntity<int>
    {
        public int Value { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}