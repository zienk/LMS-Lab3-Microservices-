using LMS.Identity.Domain.Entities;
using LMS.Identity.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS.Identity.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context;

    public RefreshTokenRepository(IdentityDbContext context) => _context = context;

    public async Task<RefreshToken?> GetActiveTokenAsync(string token)
        => await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);

    public async Task AddAsync(RefreshToken refreshToken)
        => await _context.RefreshTokens.AddAsync(refreshToken);

    public async Task RevokeAsync(string token)
    {
        var rt = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
        if (rt != null)
        {
            rt.IsRevoked = true;
            rt.RevokedAt = DateTime.UtcNow;
        }
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
