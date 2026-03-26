using LearningPlatformSystem.Models.CoreModels;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatformSystem.Context
{
    public class MyContext : IdentityDbContext<AuthUsers>
    {
        //for migration 
        public DbSet<Course> Courses { get; set; }
        public DbSet<Learner> Learners { get; set; }
        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<EnrollmentRequest> EnrollmentRequests { get; set; }
        public MyContext(DbContextOptions<MyContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Learners)
                .WithMany(l => l.Courses)
                .UsingEntity(j => j.ToTable("CourseLearners"));

        }
    }
}
