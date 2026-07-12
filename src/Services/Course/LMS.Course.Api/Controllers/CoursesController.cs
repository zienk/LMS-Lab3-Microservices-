using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Course.Api.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _courseService.GetAllAsync(page, size);
        return Ok(ApiResponse<PagedResult<CourseDTO>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _courseService.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse.Fail($"Course {id} not found."));
        return Ok(ApiResponse<CourseDTO>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.CourseId }, ApiResponse<CourseDTO>.Ok(result, "Course created."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.UpdateAsync(id, request);
        return Ok(ApiResponse<CourseDTO>.Ok(result, "Course updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Course deleted."));
    }

    [HttpPost("{id:int}/enroll")]
    public async Task<IActionResult> Enroll(int id, [FromBody] CreateEnrollmentRequest request)
    {
        var result = await _enrollmentService.EnrollAsync(id, request);
        return StatusCode(201, ApiResponse<EnrollmentDTO>.Ok(result, "Enrolled successfully."));
    }

    [HttpGet("{id:int}/enrollments")]
    public async Task<IActionResult> GetEnrollments(int id, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _enrollmentService.GetAllByCourseIdAsync(id, page, size);
        return Ok(ApiResponse<PagedResult<EnrollmentDTO>>.Ok(result));
    }
}
