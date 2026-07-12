using LMS.Student.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.Infrastructure;

public class StudentDbContext : DbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options) { }

    public DbSet<Domain.Entities.Student> Students => Set<Domain.Entities.Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.Student>(e =>
        {
            e.HasKey(s => s.StudentId);
            e.Property(s => s.FullName).HasMaxLength(100).IsRequired();
            e.Property(s => s.Email).HasMaxLength(100).IsRequired();
            e.Property(s => s.StudentCode).HasMaxLength(20).IsRequired();
            e.HasIndex(s => s.StudentCode).IsUnique();
            e.HasIndex(s => s.Email).IsUnique();
        });

        // Seed 50 students
        var students = new List<Domain.Entities.Student>();
        var names = new[] { "Nguyen Van An", "Tran Thi Binh", "Le Van Cuong", "Pham Thi Dung", "Hoang Van Em",
            "Nguyen Thi Phuong", "Tran Van Quang", "Le Thi Hoa", "Pham Van Khanh", "Do Thi Lan",
            "Nguyen Van Long", "Tran Thi Mai", "Le Van Nhat", "Pham Thi Oanh", "Hoang Van Phuc",
            "Nguyen Thi Quynh", "Tran Van Rung", "Le Thi Son", "Pham Van Thanh", "Do Thi Uyen",
            "Nguyen Van Viet", "Tran Thi Xuan", "Le Van Yen", "Pham Thi Anh", "Hoang Van Bac",
            "Nguyen Thi Cam", "Tran Van Dang", "Le Thi Giang", "Pham Van Hung", "Do Thi Kien",
            "Nguyen Van Loc", "Tran Thi Minh", "Le Van Nam", "Pham Thi Phi", "Hoang Van Quan",
            "Nguyen Thi Roi", "Tran Van Sang", "Le Thi Tam", "Pham Van Ung", "Do Thi Vang",
            "Nguyen Van Xong", "Tran Thi Yen", "Le Van Zung", "Pham Thi An", "Hoang Van Bon",
            "Nguyen Thi Cuc", "Tran Van Dat", "Le Thi Gio", "Pham Van Hai", "Do Thi It" };

        var prefixes = new[] { "SE", "CE", "SA", "QE" };
        for (int i = 1; i <= 50; i++)
        {
            var prefix = prefixes[(i - 1) % 4];
            students.Add(new Domain.Entities.Student
            {
                StudentId = i,
                FullName = names[i - 1],
                Email = $"student{i:D2}@lms.edu.vn",
                StudentCode = $"{prefix}{190000 + i:D6}",
                DateOfBirth = new DateTime(2002, ((i % 12) + 1), Math.Min(i % 28 + 1, 28), 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        modelBuilder.Entity<Domain.Entities.Student>().HasData(students);
    }
}
