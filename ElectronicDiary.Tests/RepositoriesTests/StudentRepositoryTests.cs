using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using ElectronicDiary.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace ElectronicDiary.Tests.RepositoriesTests
{
    public class StudentRepositoryTests
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

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "test-user")
    };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            httpContextAccessor.HttpContext = httpContext;

            return new ApplicationDbContext(options, httpContextAccessor);
        }

        [Fact]
        public async Task AddAsync_Should_Add_New_Student()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new StudentRepository(context);
            var student = new Student { Id = 1, FullName = "John Doe", ApplicationUserId = "test" };

            // Act
            await repository.AddAsync(student);

            // Assert
            var result = await repository.GetByIdAsync(student.Id);
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Student()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var student = new Student { Id = 1, FullName = "Jane Doe", ApplicationUserId = "test" };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var repository = new StudentRepository(context);

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jane Doe", result.FullName);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Students()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var student1 = new Student { Id = 1, FullName = "John Doe", ApplicationUserId = "test" };
            var student2 = new Student { Id = 2, FullName = "Jane Doe", ApplicationUserId = "test" };
            context.Students.AddRange(student1, student2);
            await context.SaveChangesAsync();

            var repository = new StudentRepository(context);

            // Act
            var students = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, students.Count());
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Student()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var student = new Student { Id = 1, FullName = "John Doe", ApplicationUserId = "test" };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var repository = new StudentRepository(context);

            // Act
            student.FullName = "John Updated";
            await repository.UpdateAsync(student);

            // Assert
            var updatedStudent = await repository.GetByIdAsync(student.Id);
            Assert.Equal("John Updated", updatedStudent.FullName);
        }

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Student()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var student = new Student { Id = 1, FullName = "John Doe", ApplicationUserId = "test" };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var repository = new StudentRepository(context);

            // Act
            await repository.DeleteAsync(student.Id);

            // Assert
            var students = await repository.GetAllAsync();
            Assert.DoesNotContain(students, s => s.Id == student.Id);

            var deletedStudent = await context.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == student.Id);
            Assert.NotNull(deletedStudent.DeletedAt);
        }

        [Fact]
        public async Task GetStudentsByTeacherIdAsync_Should_Return_Students_Assigned_To_Teacher()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new StudentRepository(context);

            var teacher = new Teacher { Id = 1, FullName = "Teacher 1", Subject = "test subject", ApplicationUserId = "teacher1" };
            context.Teachers.Add(teacher);

            var student1 = new Student { Id = 1, FullName = "Student 1", ApplicationUserId = "test" };
            var student2 = new Student { Id = 2, FullName = "Student 2", ApplicationUserId = "test" };
            var student3 = new Student { Id = 3, FullName = "Student 3", ApplicationUserId = "test" };
            context.Students.AddRange(student1, student2, student3);

            var teacherStudent1 = new TeacherStudent { TeacherId = teacher.Id, StudentId = student1.Id };
            var teacherStudent2 = new TeacherStudent { TeacherId = teacher.Id, StudentId = student2.Id };
            context.TeacherStudents.AddRange(teacherStudent1, teacherStudent2);

            await context.SaveChangesAsync();

            // Act
            var students = await repository.GetStudentsByTeacherIdAsync("teacher1");

            // Assert: 
            var studentIds = students.Select(s => s.Id).ToList();
            Assert.Contains(student1.Id, studentIds);
            Assert.Contains(student2.Id, studentIds);
            Assert.DoesNotContain(student3.Id, studentIds);
        }

        [Fact]
        public async Task GetStudentsByTeacherIdAsync_Should_Return_Empty_If_Teacher_Not_Found()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new StudentRepository(context);

            // Act
            var students = await repository.GetStudentsByTeacherIdAsync("nonexistent_teacher");

            // Assert
            Assert.Empty(students);
        }
    }
}
