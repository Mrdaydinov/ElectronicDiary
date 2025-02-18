namespace ElectronicDiary.Application.Interfaces
{
    using ElectronicDiary.Domain.Entities;

    public interface IAssignmentRepository
    {
        Task<Assignment> GetByIdAsync(int id);
        Task<IEnumerable<Assignment>> GetAllAsync();
        Task AddAsync(Assignment assignment);
        Task UpdateAsync(Assignment assignment);
        Task DeleteAsync(int id);
        Task<IEnumerable<Assignment>> GetAssignmentsByTeacherIdAsync(string userId);
    }
}