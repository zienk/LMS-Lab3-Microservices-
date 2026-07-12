using LMS.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Identity.Infrastructure;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.UserId);
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.Email).HasMaxLength(100).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).HasMaxLength(256).IsRequired();
            e.HasOne(r => r.User).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UserId);
            e.Ignore(r => r.IsExpired);
            e.Ignore(r => r.IsActive);
        });

        // Seed admin user: admin / 123456
        // BCrypt hash of "123456"
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 1,
            Username = "admin",
            Email = "admin@lms.edu.vn",
            PasswordHash = adminPasswordHash,
            Role = "Admin",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
