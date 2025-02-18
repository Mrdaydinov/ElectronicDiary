namespace ElectronicDiary.WebApi.DTOs
{
    public class AssignmentDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int TeacherId { get; set; }
    }
}
