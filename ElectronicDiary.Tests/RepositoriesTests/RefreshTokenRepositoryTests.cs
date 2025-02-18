using ElectronicDiary.Domain.Entities;
using ElectronicDiary.Infrastructure.Data;
using ElectronicDiary.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace ElectronicDiary.Tests.RepositoriesTests
{
    public class RefreshTokenRepositoryTests
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
        public async Task AddAsync_Should_Add_New_RefreshToken()
        {
            //Arrange
            var dbname = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbname);
            var repository = new RefreshTokenRepository(context);
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "some refresh token for test",
                UserId = "test",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            
            //Act
            await repository.AddAsync(refreshToken);

            //Assert
            var result = await repository.GetByTokenAsync("some refresh token for test");
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByTokenAsync_Should_Return_Correct_RefreshToken()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new RefreshTokenRepository(context);
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "token123",
                UserId = "testUser",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            await repository.AddAsync(refreshToken);

            // Act
            var result = await repository.GetByTokenAsync("token123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("testUser", result.UserId);
        }

        [Fact]
        public async Task GetByTokenAsync_Should_Return_Null_For_Invalid_Token()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new RefreshTokenRepository(context);

            // Act
            var result = await repository.GetByTokenAsync("nonexistent token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_RefreshToken()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repository = new RefreshTokenRepository(context);
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "oldToken",
                UserId = "testUser",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            await repository.AddAsync(refreshToken);

            // Act
            refreshToken.Token = "newToken";
            refreshToken.Expires = DateTime.UtcNow.AddDays(14);
            await repository.UpdateAsync(refreshToken);

            // Assert
            var updatedRefreshToken = await repository.GetByTokenAsync("newToken");
            Assert.NotNull(updatedRefreshToken);
            Assert.Equal("newToken", updatedRefreshToken.Token);
            Assert.True(updatedRefreshToken.Expires > DateTime.UtcNow.AddDays(10));
        }
    }
}