using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Course.Api.Controllers;

/// <summary>
/// Nhóm API tra cứu môn học.
/// </summary>
[ApiController]
[Route("api/subjects")]
[Route("api/v{version:apiVersion}/subjects")]
[ApiVersion("1.0")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService) => _subjectService = subjectService;

    /// <summary>
    /// Lấy danh sách môn học.
    /// </summary>
    /// <remarks>
    /// API cần JWT hợp lệ.
    /// Dữ liệu môn học được seed trong Course DB, tối thiểu 10 môn theo yêu cầu Lab 1.
    /// Hỗ trợ phân trang bằng `page` và `size`.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _subjectService.GetAllAsync(page, size);
        return Ok(ApiResponse<PagedResult<SubjectDTO>>.Ok(result));
    }
}
