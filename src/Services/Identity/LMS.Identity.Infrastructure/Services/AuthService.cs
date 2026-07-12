using LMS.Identity.Application.DTOs;
using LMS.Identity.Application.Services;
using LMS.Identity.Domain.Entities;
using LMS.Identity.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LMS.Identity.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepo, IRefreshTokenRepository refreshRepo, IConfiguration config)
    {
        _userRepository = userRepo;
        _refreshTokenRepository = refreshRepo;
        _configuration = config;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsAsync(request.Username))
            throw new InvalidOperationException($"Username '{request.Username}' already exists.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetActiveTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        await _refreshTokenRepository.RevokeAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return await GenerateTokensAsync(token.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenRepository.RevokeAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();
    }

    private async Task<AuthResponse> GenerateTokensAsync(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "super-secret-key-min-32-characters!!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "LMS.Identity";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "LMS.Client";
        var expireMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate refresh token
        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = expireMinutes * 60
        };
    }
}
