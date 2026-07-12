using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Course.Api.Controllers;

/// <summary>
/// Nhóm API quản lý khóa học và luồng ghi danh.
/// </summary>
[ApiController]
[Route("api/courses")]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("1.0")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public CoursesController(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Lấy danh sách khóa học.
    /// </summary>
    /// <remarks>
    /// API cần JWT hợp lệ.
    /// Hỗ trợ phân trang bằng `page` và `size`.
    /// Dữ liệu khóa học nằm trong Course DB, tách riêng khỏi Student DB và Identity DB.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _courseService.GetAllAsync(page, size);
        return Ok(ApiResponse<PagedResult<CourseDTO>>.Ok(result));
    }

    /// <summary>
    /// Lấy chi tiết khóa học.
    /// </summary>
    /// <remarks>
    /// Trả về `404 Not Found` nếu không tìm thấy khóa học.
    /// Endpoint dùng route constraint `{id:int}`.
    /// </remarks>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _courseService.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse.Fail($"Course {id} not found."));
        return Ok(ApiResponse<CourseDTO>.Ok(result));
    }

    /// <summary>
    /// Tạo khóa học mới.
    /// </summary>
    /// <remarks>
    /// Chỉ tài khoản role `Admin` được gọi API này.
    /// `semesterId` và `subjectId` phải trỏ tới dữ liệu seed trong Course DB.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.CourseId }, ApiResponse<CourseDTO>.Ok(result, "Course created."));
    }

    /// <summary>
    /// Cập nhật khóa học.
    /// </summary>
    /// <remarks>
    /// Chỉ tài khoản role `Admin` được gọi API này.
    /// Nếu khóa học không tồn tại, service trả lỗi `404 Not Found`.
    /// </remarks>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.UpdateAsync(id, request);
        return Ok(ApiResponse<CourseDTO>.Ok(result, "Course updated."));
    }

    /// <summary>
    /// Xóa khóa học.
    /// </summary>
    /// <remarks>
    /// Chỉ tài khoản role `Admin` được gọi API này.
    /// Dùng khi cần xóa course khỏi Course DB.
    /// </remarks>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Course deleted."));
    }

    /// <summary>
    /// Ghi danh sinh viên vào khóa học.
    /// </summary>
    /// <remarks>
    /// Đây là business flow chính của Lab 3.
    /// Course Service sẽ gọi gRPC sang Student Service để kiểm tra `studentId` tồn tại trước khi tạo Enrollment.
    /// Không có truy vấn trực tiếp sang Student DB.
    /// Lỗi thường gặp: `404` nếu student hoặc course không tồn tại, `400` nếu sinh viên đã ghi danh course này.
    /// </remarks>
    [HttpPost("{id:int}/enroll")]
    public async Task<IActionResult> Enroll(int id, [FromBody] CreateEnrollmentRequest request)
    {
        var result = await _enrollmentService.EnrollAsync(id, request);
        return StatusCode(201, ApiResponse<EnrollmentDTO>.Ok(result, "Enrolled successfully."));
    }

    /// <summary>
    /// Lấy danh sách ghi danh của một khóa học.
    /// </summary>
    /// <remarks>
    /// Trả về các enrollment thuộc course ID truyền trên route.
    /// Hỗ trợ phân trang bằng `page` và `size`.
    /// </remarks>
    [HttpGet("{id:int}/enrollments")]
    public async Task<IActionResult> GetEnrollments(int id, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _enrollmentService.GetAllByCourseIdAsync(id, page, size);
        return Ok(ApiResponse<PagedResult<EnrollmentDTO>>.Ok(result));
    }
}
