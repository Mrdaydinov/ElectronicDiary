namespace ElectronicDiary.WebApi.DTOs
{
    public class GradeDto
    {
        public int StudentId { get; set; }
        public int AssignmentId { get; set; }
        public int Value { get; set; }
        public DateTime DateGiven { get; set; }
    }
}
