using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Course.Api.Controllers;

/// <summary>
/// Nhóm API tra cứu học kỳ.
/// </summary>
[ApiController]
[Route("api/semesters")]
[Route("api/v{version:apiVersion}/semesters")]
[ApiVersion("1.0")]
[Authorize]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService) => _semesterService = semesterService;

    /// <summary>
    /// Lấy danh sách học kỳ.
    /// </summary>
    /// <remarks>
    /// API cần JWT hợp lệ.
    /// Dữ liệu học kỳ được seed trong Course DB, tối thiểu 5 học kỳ theo yêu cầu Lab 1.
    /// Hỗ trợ phân trang bằng `page` và `size`.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _semesterService.GetAllAsync(page, size);
        return Ok(ApiResponse<PagedResult<SemesterDTO>>.Ok(result));
    }
}
