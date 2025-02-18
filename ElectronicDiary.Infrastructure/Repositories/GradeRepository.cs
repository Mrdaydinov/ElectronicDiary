using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class GradeRepository : IGradeRepository
{
    private readonly ApplicationDbContext _context;

    public GradeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Grade> GetByIdAsync(int id)
    {
        return await _context.Grades.FindAsync(id);
    }

    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        return await _context.Grades.ToListAsync();
    }


    public async Task AddAsync(Grade grade)
    {
        await _context.Grades.AddAsync(grade);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Grade grade)
    {
        _context.Grades.Update(grade);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade != null)
        {
            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
        }
    }

    //

    public async Task<IEnumerable<Grade>> GetGradesByStudentIdAsync(string userId)
    {
        var student = await _context.Students.Include(s => s.Grades).FirstOrDefaultAsync(t => t.ApplicationUserId == userId);
        if (student == null)
            return Enumerable.Empty<Grade>();
        return student.Grades;
    }

    public async Task<IEnumerable<Grade>> GetGradesByAssignmentIdAsync(int assignmentId)
    {
        var grades = await _context.Grades.Include(g=>g.Assignment).Where(g=>g.AssignmentId == assignmentId).ToListAsync();

        return grades;
    }
}