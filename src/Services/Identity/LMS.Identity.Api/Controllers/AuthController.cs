using LMS.Identity.Application.DTOs;
using LMS.Identity.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Identity.Api.Controllers;

/// <summary>
/// Nhóm API xác thực người dùng: đăng ký, đăng nhập, cấp lại token và đăng xuất.
/// </summary>
[ApiController]
[Route("api/auth")]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Đăng nhập và lấy JWT.
    /// </summary>
    /// <remarks>
    /// Dùng tài khoản seed để demo: username `admin`, password `123456`.
    /// Sau khi login thành công, copy `accessToken` rồi bấm Authorize trên Swagger để gọi các API cần đăng nhập.
    /// Endpoint có cả route không version `/api/auth/login` và route version `/api/v1/auth/login`.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }

    /// <summary>
    /// Đăng ký tài khoản mới.
    /// </summary>
    /// <remarks>
    /// Role hợp lệ là `Student` hoặc `Admin`.
    /// Mật khẩu không lưu plain text; Identity Service hash mật khẩu bằng BCrypt trước khi lưu vào database.
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Registration successful."));
    }

    /// <summary>
    /// Cấp lại access token bằng refresh token.
    /// </summary>
    /// <remarks>
    /// Gửi refresh token còn hiệu lực để lấy access token mới.
    /// Refresh token cũ sẽ bị thu hồi để tránh dùng lại nhiều lần.
    /// </remarks>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed."));
    }

    /// <summary>
    /// Đăng xuất và thu hồi refresh token.
    /// </summary>
    /// <remarks>
    /// Dùng khi client muốn kết thúc phiên đăng nhập.
    /// Sau khi logout, refresh token trong request sẽ không còn dùng để cấp token mới được nữa.
    /// </remarks>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }
}
