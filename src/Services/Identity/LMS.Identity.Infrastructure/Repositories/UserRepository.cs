using LMS.Identity.Domain.Entities;
using LMS.Identity.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context) => _context = context;

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<bool> ExistsAsync(string username)
        => await _context.Users.AnyAsync(u => u.Username == username);

    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
