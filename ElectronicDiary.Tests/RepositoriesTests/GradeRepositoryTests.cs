using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronicDiary.Tests.RepositoriesTests
{
    public class GradeRepositoryTests
    {
        private ApplicationDbContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var httpContextAccessor = new HttpContextAccessor();
            var httpContext = new DefaultHttpContext();

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            httpContext.RequestServices = serviceProvider;
            httpContextAccessor.HttpContext = httpContext;

            return new ApplicationDbContext(options, httpContextAccessor);
        }

        [Fact]
        public async Task AddAsync_Should_Add_New_Grade()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new GradeRepository(context);
            var grade = new Grade
            {
                Id = 1,
                Value = 95,
                AssignmentId = 1
            };

            // Act
            await repository.AddAsync(grade);

            // Assert
            var result = await repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(95, result.Value);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Grade()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var grade = new Grade { Id = 1, Value = 80, AssignmentId = 1 };
            context.Grades.Add(grade);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(80, result.Value);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Grades()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var grade1 = new Grade { Id = 1, Value = 90, AssignmentId = 1 };
            var grade2 = new Grade { Id = 2, Value = 85, AssignmentId = 1 };
            context.Grades.AddRange(grade1, grade2);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            var grades = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, grades.Count());
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Grade()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var grade = new Grade { Id = 1, Value = 75, AssignmentId = 1 };
            context.Grades.Add(grade);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            grade.Value = 85;
            await repository.UpdateAsync(grade);

            // Assert
            var updatedGrade = await repository.GetByIdAsync(1);
            Assert.Equal(85, updatedGrade.Value);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Grade()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var grade = new Grade { Id = 1, Value = 70, AssignmentId = 1 };
            context.Grades.Add(grade);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            await repository.DeleteAsync(1);

            // Assert
            var grades = await repository.GetAllAsync();
            Assert.DoesNotContain(grades, s => s.Id == grade.Id);

            var deletedGrade = await context.Grades
            .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == grade.Id);
            Assert.NotNull(deletedGrade.DeletedAt);
        }

        [Fact]
        public async Task GetGradesByStudentIdAsync_Should_Return_Grades_For_Student()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            var student = new Student
            {
                Id = 1,
                FullName = "Student 1",
                ApplicationUserId = "student1",
                Grades = new List<Grade>()
            };
            context.Students.Add(student);

            var grade1 = new Grade { Id = 1, Value = 88, AssignmentId = 1, StudentId = student.Id };
            var grade2 = new Grade { Id = 2, Value = 92, AssignmentId = 2, StudentId = student.Id };

            student.Grades.Add(grade1);
            student.Grades.Add(grade2);

            context.Grades.AddRange(grade1, grade2);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            var grades = await repository.GetGradesByStudentIdAsync("student1");

            // Assert
            var gradeIds = grades.Select(g => g.Id).ToList();
            Assert.Contains(1, gradeIds);
            Assert.Contains(2, gradeIds);
        }

        [Fact]
        public async Task GetGradesByStudentIdAsync_Should_Return_Empty_If_Student_Not_Found()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            var grades = await repository.GetGradesByStudentIdAsync("nonexistent_student");

            // Assert
            Assert.Empty(grades);
        }

        [Fact]
        public async Task GetGradesByAssignmentIdAsync_Should_Return_Grades_For_Assignment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            var assignment10 = new Assignment
            {
                Id = 10,
                Title = "Test Assignment 10",
                Description = "Test Description"
            };
            var assignment11 = new Assignment
            {
                Id = 11,
                Title = "Test Assignment 11",
                Description = "Test Description"
            };
            context.Assignments.AddRange(assignment10, assignment11);

            var grade1 = new Grade { Id = 1, Value = 78, AssignmentId = 10 };
            var grade2 = new Grade { Id = 2, Value = 82, AssignmentId = 10 };
            var grade3 = new Grade { Id = 3, Value = 90, AssignmentId = 11 };

            context.Grades.AddRange(grade1, grade2, grade3);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            var gradesForAssignment = await repository.GetGradesByAssignmentIdAsync(10);

            // Assert
            var gradeIds = gradesForAssignment.Select(g => g.Id).ToList();
            Assert.Contains(1, gradeIds);
            Assert.Contains(2, gradeIds);
            Assert.DoesNotContain(3, gradeIds);
        }


        [Fact]
        public async Task GetGradesByAssignmentIdAsync_Should_Return_Empty_If_No_Grades_Found()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            await context.SaveChangesAsync();

            var repository = new GradeRepository(context);

            // Act
            var gradesForAssignment = await repository.GetGradesByAssignmentIdAsync(999);

            // Assert
            Assert.Empty(gradesForAssignment);
        }
    }
}
