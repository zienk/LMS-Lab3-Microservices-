using LMS.Identity.Domain.Entities;

namespace LMS.Identity.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetActiveTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAsync(string token);
    Task SaveChangesAsync();
}
