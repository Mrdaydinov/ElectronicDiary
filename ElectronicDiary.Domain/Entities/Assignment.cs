using ElectronicDiary.Domain.Common.Concretes;

namespace ElectronicDiary.Domain.Entities
{
    public class Assignment : BaseEntity<int>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        public ICollection<StudentAssignment> StudentAssignments { get; set; }
    }
}