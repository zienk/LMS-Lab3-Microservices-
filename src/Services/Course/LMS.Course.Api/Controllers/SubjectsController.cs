using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Course.Api.Controllers;

[ApiController]
[Route("api/subjects")]
[Route("api/v{version:apiVersion}/subjects")]
[ApiVersion("1.0")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService) => _subjectService = subjectService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _subjectService.GetAllAsync(page, size);
        return Ok(ApiResponse<PagedResult<SubjectDTO>>.Ok(result));
    }
}
