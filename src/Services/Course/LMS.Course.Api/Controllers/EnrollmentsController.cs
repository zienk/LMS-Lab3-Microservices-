using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Course.Api.Controllers;

/// <summary>
/// Nhóm API ghi danh tương thích checklist Lab 2.
/// </summary>
[ApiController]
[Route("api/enrollments")]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("1.0")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Tạo ghi danh bằng body chứa studentId và courseId.
    /// </summary>
    /// <remarks>
    /// Endpoint này phục vụ checklist `POST /api/v1/enrollments`.
    /// Dù route khác với `/api/courses/{id}/enroll`, service vẫn dùng cùng business rule:
    /// kiểm tra course trong Course DB, gọi gRPC sang Student Service để xác minh student, rồi mới tạo Enrollment.
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentDirectRequest request)
    {
        var result = await _enrollmentService.EnrollAsync(request);
        return StatusCode(201, ApiResponse<EnrollmentDTO>.Ok(result, "Enrolled successfully."));
    }
}
