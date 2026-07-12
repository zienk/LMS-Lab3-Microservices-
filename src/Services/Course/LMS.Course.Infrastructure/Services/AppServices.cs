using LMS.Course.Application.DTOs;
using LMS.Course.Application.Services;
using LMS.Course.Domain.Entities;
using LMS.Course.Domain.Interfaces;
using LMS.Shared.Contracts.Common;

namespace LMS.Course.Infrastructure.Services;

public class CourseAppService : ICourseService
{
    private readonly ICourseRepository _repo;

    public CourseAppService(ICourseRepository repo) => _repo = repo;

    public async Task<PagedResult<CourseDTO>> GetAllAsync(int page, int size)
    {
        var result = await _repo.GetAllAsync(page, size);
        return new PagedResult<CourseDTO>
        {
            Data = result.Data.Select(c => new CourseDTO
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId,
                SubjectId = c.SubjectId
            }),
            Pagination = result.Pagination
        };
    }

    public async Task<CourseDTO?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        if (c == null) return null;
        return new CourseDTO
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName,
            SemesterId = c.SemesterId,
            SubjectId = c.SubjectId
        };
    }

    public async Task<CourseDTO> CreateAsync(CreateCourseRequest request)
    {
        var c = new CourseEntity
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        };
        await _repo.AddAsync(c);
        await _repo.SaveChangesAsync();
        return new CourseDTO
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName,
            SemesterId = c.SemesterId,
            SubjectId = c.SubjectId
        };
    }

    public async Task<CourseDTO> UpdateAsync(int id, CreateCourseRequest request)
    {
        var course = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        course.CourseName = request.CourseName;
        course.SemesterId = request.SemesterId;
        course.SubjectId = request.SubjectId;

        await _repo.UpdateAsync(course);
        await _repo.SaveChangesAsync();

        return new CourseDTO
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId,
            SubjectId = course.SubjectId
        };
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _repo.ExistsAsync(id))
            throw new KeyNotFoundException($"Course {id} not found.");

        await _repo.DeleteAsync(id);
        await _repo.SaveChangesAsync();
    }
}

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repo;
    public SemesterService(ISemesterRepository repo) => _repo = repo;

    public async Task<PagedResult<SemesterDTO>> GetAllAsync(int page, int size)
    {
        var result = await _repo.GetAllAsync(page, size);
        return new PagedResult<SemesterDTO>
        {
            Data = result.Data.Select(s => new SemesterDTO
            {
                SemesterId = s.SemesterId,
                SemesterName = s.SemesterName,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            }),
            Pagination = result.Pagination
        };
    }
}

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;
    public SubjectService(ISubjectRepository repo) => _repo = repo;

    public async Task<PagedResult<SubjectDTO>> GetAllAsync(int page, int size)
    {
        var result = await _repo.GetAllAsync(page, size);
        return new PagedResult<SubjectDTO>
        {
            Data = result.Data.Select(s => new SubjectDTO
            {
                SubjectId = s.SubjectId,
                SubjectCode = s.SubjectCode,
                SubjectName = s.SubjectName,
                Credit = s.Credit
            }),
            Pagination = result.Pagination
        };
    }
}

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IStudentGrpcClient _studentGrpcClient;

    public EnrollmentService(IEnrollmentRepository enrollmentRepo, ICourseRepository courseRepo, IStudentGrpcClient studentGrpcClient)
    {
        _enrollmentRepo = enrollmentRepo;
        _courseRepo = courseRepo;
        _studentGrpcClient = studentGrpcClient;
    }

    public async Task<PagedResult<EnrollmentDTO>> GetAllByCourseIdAsync(int courseId, int page, int size)
    {
        var result = await _enrollmentRepo.GetAllByCourseIdAsync(courseId, page, size);
        return new PagedResult<EnrollmentDTO>
        {
            Data = result.Data.Select(e => new EnrollmentDTO
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }),
            Pagination = result.Pagination
        };
    }

    public async Task<EnrollmentDTO> EnrollAsync(int courseId, CreateEnrollmentRequest request)
    {
        // 1. Verify Course Exists
        if (!await _courseRepo.ExistsAsync(courseId))
            throw new KeyNotFoundException($"Course {courseId} not found.");

        // 2. Verify Student Exists via gRPC
        var studentExists = await _studentGrpcClient.CheckStudentExistsAsync(request.StudentId);
        if (!studentExists)
            throw new KeyNotFoundException($"Student {request.StudentId} not found in Student Service.");

        // 3. Check if already enrolled
        if (await _enrollmentRepo.ExistsAsync(request.StudentId, courseId))
            throw new InvalidOperationException($"Student {request.StudentId} is already enrolled in course {courseId}.");

        // 4. Create enrollment
        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = courseId,
            EnrollDate = DateTime.UtcNow,
            Status = "Active"
        };

        await _enrollmentRepo.AddAsync(enrollment);
        await _enrollmentRepo.SaveChangesAsync();

        return new EnrollmentDTO
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };
    }

    public Task<EnrollmentDTO> EnrollAsync(CreateEnrollmentDirectRequest request)
        => EnrollAsync(request.CourseId, new CreateEnrollmentRequest { StudentId = request.StudentId });
}
