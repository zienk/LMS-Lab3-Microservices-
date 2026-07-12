using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Course.Api.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentDirectRequest request)
    {
        var result = await _enrollmentService.EnrollAsync(request);
        return StatusCode(201, ApiResponse<EnrollmentDTO>.Ok(result, "Enrolled successfully."));
    }
}
