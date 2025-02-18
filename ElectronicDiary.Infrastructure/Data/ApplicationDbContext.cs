using ElectronicDiary.Domain.Common.Abstracts;
using ElectronicDiary.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ElectronicDiary.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
             : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<StudentAssignment> StudentAssignments { get; set; }
        public DbSet<TeacherStudent> TeacherStudents { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>().HasQueryFilter(e => !e.DeletedAt.HasValue);
            modelBuilder.Entity<Teacher>().HasQueryFilter(e => !e.DeletedAt.HasValue);
            modelBuilder.Entity<Assignment>().HasQueryFilter(e => !e.DeletedAt.HasValue);
            modelBuilder.Entity<Grade>().HasQueryFilter(e => !e.DeletedAt.HasValue);
        }




        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changes = ChangeTracker.Entries<IAuditableEntity>().Where(c => c.State == EntityState.Added || c.State == EntityState.Modified || c.State == EntityState.Deleted);

            var currentTime = DateTime.UtcNow;

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                userId = "system";
            }


            foreach (var entry in changes)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = currentTime;
                        entry.Entity.CreatedBy = userId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = currentTime;
                        entry.Entity.ModifiedBy = userId;

                        entry.Property(m => m.CreatedAt).IsModified = false;
                        entry.Property(m => m.CreatedBy).IsModified = false;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; 
                        entry.Entity.DeletedAt = currentTime;
                        entry.Entity.DeletedBy = userId;

                        entry.Property(m => m.CreatedAt).IsModified = false;
                        entry.Property(m => m.CreatedBy).IsModified = false;
                        entry.Property(m => m.ModifiedAt).IsModified = false;
                        entry.Property(m => m.ModifiedBy).IsModified = false;
                        break;

                    default:
                        break;
                }
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var logger = _httpContextAccessor.HttpContext?.RequestServices.GetService<ILogger<ApplicationDbContext>>();
                logger?.LogError(ex, "An error occurred while saving changes to the database");
                throw;
            }
        }

    }
}
