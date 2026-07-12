using LMS.Identity.Domain.Entities;

namespace LMS.Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(string username);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
