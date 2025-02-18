namespace ElectronicDiary.Application.Interfaces
{
    using ElectronicDiary.Domain.Entities;

    public interface IGradeRepository
    {
        Task<Grade> GetByIdAsync(int id);
        Task<IEnumerable<Grade>> GetAllAsync();
        Task AddAsync(Grade grade);
        Task UpdateAsync(Grade grade);
        Task DeleteAsync(int id);
        Task<IEnumerable<Grade>> GetGradesByStudentIdAsync(string userId);
        Task<IEnumerable<Grade>> GetGradesByAssignmentIdAsync(int assignmentId);
    }
}
