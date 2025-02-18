using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using ElectronicDiary.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace ElectronicDiary.Tests.RepositoriesTests
{
    public class TeacherRepositoryTests
    {
        private ApplicationDbContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
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
        public async Task GetAllAsync_Should_Return_All_Teachers()
        {
            //Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new TeacherRepository(context);

            var teacher1 = new Teacher { Id = 1, FullName = "John Doe", Subject = "test subject", ApplicationUserId = "test" };
            var teacher2 = new Teacher { Id = 2, FullName = "John Doe 2", Subject = "test subject 2", ApplicationUserId = "test" };
            await context.Teachers.AddRangeAsync(teacher1, teacher2);
            await context.SaveChangesAsync();

            //Act
            var teachers = await repository.GetAllAsync();

            //Assert

            Assert.Equal(2, teachers.Count());
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Teacher()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new TeacherRepository(context);

            var teacher = new Teacher { Id = 1, FullName = "John Doe", Subject = "test subject", ApplicationUserId = "test" };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.FullName);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Teacher()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new TeacherRepository(context);

            var teacher = new Teacher { Id = 1, FullName = "John Doe", Subject = "test subject", ApplicationUserId = "test" };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            // Act
            teacher.FullName = "John Updated";
            await repository.UpdateAsync(teacher);

            // Assert
            var updatedTeacher = await repository.GetByIdAsync(teacher.Id);
            Assert.Equal("John Updated", updatedTeacher.FullName);
        }

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Teacher()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new TeacherRepository(context);

            var teacher = new Teacher { Id = 1, FullName = "John Doe", Subject = "test subject", ApplicationUserId = "test" };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(teacher.Id);

            // Assert
            var teachers = await repository.GetAllAsync();
            Assert.DoesNotContain(teachers, s => s.Id == teacher.Id);

            var deletedTeacher = await context.Teachers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == teacher.Id);
            Assert.NotNull(deletedTeacher.DeletedAt);
        }

        [Fact]
        public async Task AddAsync_Should_Add_New_Teacher()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new TeacherRepository(context);

            var teacher = new Teacher { Id = 1, FullName = "John Doe", Subject = "test subject", ApplicationUserId = "test" };


            // Act
            await repository.AddAsync(teacher);

            // Assert
            var result = await repository.GetByIdAsync(teacher.Id);
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.FullName);
        }
    }
}
