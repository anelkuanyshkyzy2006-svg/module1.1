using Microsoft.EntityFrameworkCore;
using UniversityApi.Models;

namespace UniversityApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Fluent API: индексы, связи, внешние ключи, поведение удаления ----

        modelBuilder.Entity<Student>(e =>
        {
            e.HasIndex(s => s.Email).IsUnique();
        });

        modelBuilder.Entity<Teacher>(e =>
        {
            e.HasIndex(t => t.Email).IsUnique();
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.Property(c => c.Credits).IsRequired();

            // Teacher 1 — * Course
            e.HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.Property(x => x.Grade).HasPrecision(5, 2);

            // Student 1 — * Enrollment
            e.HasOne(x => x.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course 1 — * Enrollment
            e.HasOne(x => x.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Один студент — один раз на один курс (Many-to-Many через Enrollment)
            e.HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
        });
    }
}
