using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectronicDiary.Infrastructure.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Assignment> GetByIdAsync(int id)
        {
            return await _context.Assignments.FindAsync(id);
        }

        public async Task<IEnumerable<Assignment>> GetAllAsync()
        {
            return await _context.Assignments.ToListAsync();
        }

        public async Task AddAsync(Assignment assignment)
        {
            await _context.Assignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Assignment assignment)
        {
            _context.Assignments.Update(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        //
        public async Task<IEnumerable<Assignment>> GetAssignmentsByTeacherIdAsync(string userId)
        {
            var teacher = await _context.Teachers.
                Include(t => t.Assignments).
                FirstOrDefaultAsync(t => t.ApplicationUserId == userId);

            if(teacher == null)
                return Enumerable.Empty<Assignment>();

            return teacher.Assignments;
        }
    }
}
