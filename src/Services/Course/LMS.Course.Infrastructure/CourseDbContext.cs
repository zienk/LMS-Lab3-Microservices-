using LMS.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Course.Infrastructure;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Semester>(e =>
        {
            e.HasKey(x => x.SemesterId);
            e.Property(x => x.SemesterName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Subject>(e =>
        {
            e.HasKey(x => x.SubjectId);
            e.Property(x => x.SubjectCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.SubjectName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<CourseEntity>(e =>
        {
            e.HasKey(x => x.CourseId);
            e.Property(x => x.CourseName).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Semester).WithMany().HasForeignKey(x => x.SemesterId);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId);
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.HasKey(x => x.EnrollmentId);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Course).WithMany().HasForeignKey(x => x.CourseId);
            // StudentId is just an int, no FK constraint because it's in another DB
            e.HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
        });

        // Seed Data
        modelBuilder.Entity<Semester>().HasData(
            new Semester { SemesterId = 1, SemesterName = "Fall 2024", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2024, 12, 31) },
            new Semester { SemesterId = 2, SemesterName = "Spring 2025", StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 4, 30) },
            new Semester { SemesterId = 3, SemesterName = "Summer 2025", StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 8, 31) },
            new Semester { SemesterId = 4, SemesterName = "Fall 2025", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 12, 31) },
            new Semester { SemesterId = 5, SemesterName = "Spring 2026", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 4, 30) }
        );

        modelBuilder.Entity<Subject>().HasData(
            new Subject { SubjectId = 1, SubjectCode = "PRN232", SubjectName = "Building Cloud-based APIs", Credit = 3 },
            new Subject { SubjectId = 2, SubjectCode = "PRJ301", SubjectName = "Java Web Application", Credit = 3 },
            new Subject { SubjectId = 3, SubjectCode = "SWT301", SubjectName = "Software Testing", Credit = 3 },
            new Subject { SubjectId = 4, SubjectCode = "SWM201", SubjectName = "Software Requirement", Credit = 3 },
            new Subject { SubjectId = 5, SubjectCode = "DBI202", SubjectName = "Database Systems", Credit = 3 },
            new Subject { SubjectId = 6, SubjectCode = "PRO192", SubjectName = "Object-Oriented Programming", Credit = 3 },
            new Subject { SubjectId = 7, SubjectCode = "CSD201", SubjectName = "Data Structures and Algorithms", Credit = 3 },
            new Subject { SubjectId = 8, SubjectCode = "WED201c", SubjectName = "Web Design", Credit = 3 },
            new Subject { SubjectId = 9, SubjectCode = "MAS291", SubjectName = "Statistics and Probability", Credit = 3 },
            new Subject { SubjectId = 10, SubjectCode = "JPD113", SubjectName = "Japanese 1", Credit = 3 }
        );

        var courses = new List<CourseEntity>();
        for (int i = 1; i <= 20; i++)
        {
            courses.Add(new CourseEntity
            {
                CourseId = i,
                CourseName = $"Class {(i % 5) + 1}",
                SemesterId = (i % 5) + 1,
                SubjectId = (i % 10) + 1
            });
        }
        modelBuilder.Entity<CourseEntity>().HasData(courses);
    }
}
