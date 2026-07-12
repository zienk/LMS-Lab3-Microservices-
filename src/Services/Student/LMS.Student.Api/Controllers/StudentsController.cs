using LMS.Shared.Contracts.Common;
using LMS.Student.Application.DTOs;
using LMS.Student.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Student.Api.Controllers;

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

    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetV1([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? search = null, [FromQuery] string? sort = null)
    {
        var result = await _studentService.GetAllAsync(page, size, search, sort);
        return Ok(ApiResponse<PagedResult<StudentResponse>>.Ok(result));
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetV2([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? search = null, [FromQuery] string? sort = null)
    {
        // For Lab demo: v2 can just return the same or add a "V2" flag somewhere. We just return the same.
        var result = await _studentService.GetAllAsync(page, size, search, sort);
        return Ok(ApiResponse<PagedResult<StudentResponse>>.Ok(result, "Response from API v2"));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _studentService.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse.Fail($"Student {id} not found."));
        return Ok(ApiResponse<StudentResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] StudentRequest request)
    {
        var result = await _studentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.StudentId }, ApiResponse<StudentResponse>.Ok(result, "Student created."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentRequest request)
    {
        var result = await _studentService.UpdateAsync(id, request);
        return Ok(ApiResponse<StudentResponse>.Ok(result, "Student updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _studentService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Student deleted."));
    }
}
