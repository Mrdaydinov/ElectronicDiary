using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectronicDiary.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Student> GetByIdAsync(int id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Student>> GetStudentsByTeacherIdAsync(string userId)
        {
            var teacher = await _context.Teachers
                  .FirstOrDefaultAsync(t => t.ApplicationUserId == userId);

            if (teacher == null)
                return Enumerable.Empty<Student>();

            var studentIds = await _context.TeacherStudents
                .Where(ts => ts.TeacherId == teacher.Id)
                .Select(ts => ts.StudentId)
                .ToListAsync();

            if (studentIds.Count == 0)
                return Enumerable.Empty<Student>();

            return await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();
        }
    }
}
