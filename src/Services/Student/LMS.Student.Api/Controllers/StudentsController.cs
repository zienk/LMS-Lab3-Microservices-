using LMS.Shared.Contracts.Common;
using LMS.Student.Application.DTOs;
using LMS.Student.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Student.Api.Controllers;

/// <summary>
/// Nhóm API quản lý sinh viên.
/// </summary>
[ApiController]
[Route("api/students")]
[Route("api/v{version:apiVersion}/students")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService) => _studentService = studentService;

    /// <summary>
    /// Lấy danh sách sinh viên API v1.
    /// </summary>
    /// <remarks>
    /// API cần JWT hợp lệ.
    /// Hỗ trợ phân trang bằng `page`, `size`; tìm kiếm bằng `search`; sắp xếp bằng `sort`.
    /// Ví dụ: `GET /api/v1/students?page=1&amp;size=5&amp;search=nguyen&amp;sort=fullName`.
    /// </remarks>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetV1([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? search = null, [FromQuery] string? sort = null)
    {
        var result = await _studentService.GetAllAsync(page, size, search, sort);
        return Ok(ApiResponse<PagedResult<StudentResponse>>.Ok(result));
    }

    /// <summary>
    /// Lấy danh sách sinh viên API v2.
    /// </summary>
    /// <remarks>
    /// API chứng minh versioning theo yêu cầu Lab 2.
    /// Hiện tại dữ liệu trả về giống v1 nhưng message response thể hiện đây là API v2.
    /// </remarks>
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetV2([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? search = null, [FromQuery] string? sort = null)
    {
        // For Lab demo: v2 can just return the same or add a "V2" flag somewhere. We just return the same.
        var result = await _studentService.GetAllAsync(page, size, search, sort);
        return Ok(ApiResponse<PagedResult<StudentResponse>>.Ok(result, "Response from API v2"));
    }

    /// <summary>
    /// Lấy chi tiết một sinh viên.
    /// </summary>
    /// <remarks>
    /// Trả về `404 Not Found` nếu không có sinh viên với ID tương ứng.
    /// Endpoint này dùng route constraint `{id:int}`.
    /// </remarks>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _studentService.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse.Fail($"Student {id} not found."));
        return Ok(ApiResponse<StudentResponse>.Ok(result));
    }

    /// <summary>
    /// Tạo sinh viên mới.
    /// </summary>
    /// <remarks>
    /// Chỉ tài khoản role `Admin` được gọi API này.
    /// Mã sinh viên phải đúng định dạng FPTU, ví dụ `SE190001`, `CE190002`.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] StudentRequest request)
    {
        var result = await _studentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.StudentId }, ApiResponse<StudentResponse>.Ok(result, "Student created."));
    }

    /// <summary>
    /// Cập nhật thông tin sinh viên.
    /// </summary>
    /// <remarks>
    /// Dùng để cập nhật họ tên, email, mã sinh viên và ngày sinh.
    /// Nếu ID không tồn tại, service trả lỗi `404 Not Found`.
    /// </remarks>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentRequest request)
    {
        var result = await _studentService.UpdateAsync(id, request);
        return Ok(ApiResponse<StudentResponse>.Ok(result, "Student updated."));
    }

    /// <summary>
    /// Xóa sinh viên.
    /// </summary>
    /// <remarks>
    /// Chỉ tài khoản role `Admin` được gọi API này.
    /// Nếu ID không tồn tại, service trả lỗi `404 Not Found`.
    /// </remarks>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _studentService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Student deleted."));
    }
}
