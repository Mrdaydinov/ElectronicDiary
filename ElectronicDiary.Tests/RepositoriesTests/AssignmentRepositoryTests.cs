using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using ElectronicDiary.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronicDiary.Tests.RepositoriesTests
{
    public class AssignmentRepositoryTests
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
        public async Task AddAsync_Should_Add_New_Assignment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new AssignmentRepository(context);
            var assignment = new Assignment
            {
                Id = 1,
                Title = "Test Assignment",
                Description = "Test description"
            };

            // Act
            await repository.AddAsync(assignment);

            // Assert
            var result = await repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Test Assignment", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Assignment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var assignment = new Assignment
            {
                Id = 1,
                Title = "Assignment 1",
                Description = "Test description"
            };
            context.Assignments.Add(assignment);
            await context.SaveChangesAsync();

            var repository = new AssignmentRepository(context);

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Assignment 1", result.Title);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Assignments()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var assignment1 = new Assignment { Id = 1, Title = "Assignment 1", Description = "Test description" };
            var assignment2 = new Assignment { Id = 2, Title = "Assignment 2", Description = "Test description" };
            context.Assignments.AddRange(assignment1, assignment2);
            await context.SaveChangesAsync();

            var repository = new AssignmentRepository(context);

            // Act
            var assignments = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, assignments.Count());
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Assignment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var assignment = new Assignment { Id = 1, Title = "Original Title", Description = "Test description" };
            context.Assignments.Add(assignment);
            await context.SaveChangesAsync();

            var repository = new AssignmentRepository(context);

            // Act
            assignment.Title = "Updated Title";
            await repository.UpdateAsync(assignment);

            // Assert
            var updatedAssignment = await repository.GetByIdAsync(1);
            Assert.Equal("Updated Title", updatedAssignment.Title);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Assignment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var assignment = new Assignment { Id = 1, Title = "Assignment to Delete", Description = "Test description" };
            context.Assignments.Add(assignment);
            await context.SaveChangesAsync();

            var repository = new AssignmentRepository(context);

            // Act
            await repository.DeleteAsync(1);

            // Assert
            var assignments = await repository.GetAllAsync();
            Assert.DoesNotContain(assignments, s => s.Id == assignment.Id);

            var deletedAssignment = await context.Assignments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == assignment.Id);
            Assert.NotNull(deletedAssignment.DeletedAt);
        }

        [Fact]
        public async Task GetAssignmentsByTeacherIdAsync_Should_Return_Assignments_For_Teacher()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            var teacher = new Teacher
            {
                Id = 1,
                FullName = "Teacher 1",
                Subject = "test-subject",
                ApplicationUserId = "teacher1",
                Assignments = new List<Assignment>()
            };
            context.Teachers.Add(teacher);

            var assignment1 = new Assignment { Id = 1, Title = "Assignment 1", Description = "Test description" };
            var assignment2 = new Assignment { Id = 2, Title = "Assignment 2", Description = "Test description" };
            teacher.Assignments.Add(assignment1);
            teacher.Assignments.Add(assignment2);

            context.Assignments.AddRange(assignment1, assignment2);
            await context.SaveChangesAsync();

            var repository = new AssignmentRepository(context);

            // Act
            var assignments = await repository.GetAssignmentsByTeacherIdAsync("teacher1");

            // Assert
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            Assert.Contains(1, assignmentIds);
            Assert.Contains(2, assignmentIds);
        }

        [Fact]
        public async Task GetAssignmentsByTeacherIdAsync_Should_Return_Empty_If_Teacher_Not_Found()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            await context.SaveChangesAsync();

            var repository = new AssignmentRepository(context);

            // Act
            var assignments = await repository.GetAssignmentsByTeacherIdAsync("nonexistent_teacher");

            // Assert
            Assert.Empty(assignments);
        }
    }
}
