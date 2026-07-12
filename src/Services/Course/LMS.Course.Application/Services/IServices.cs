using LMS.Course.Application.DTOs;
using LMS.Shared.Contracts.Common;

namespace LMS.Course.Application.Services;

public interface ICourseService
{
    Task<PagedResult<CourseDTO>> GetAllAsync(int page, int size);
    Task<CourseDTO?> GetByIdAsync(int id);
    Task<CourseDTO> CreateAsync(CreateCourseRequest request);
    Task<CourseDTO> UpdateAsync(int id, CreateCourseRequest request);
    Task DeleteAsync(int id);
}

public interface ISemesterService
{
    Task<PagedResult<SemesterDTO>> GetAllAsync(int page, int size);
}

public interface ISubjectService
{
    Task<PagedResult<SubjectDTO>> GetAllAsync(int page, int size);
}

public interface IEnrollmentService
{
    Task<PagedResult<EnrollmentDTO>> GetAllByCourseIdAsync(int courseId, int page, int size);
    Task<EnrollmentDTO> EnrollAsync(int courseId, CreateEnrollmentRequest request);
    Task<EnrollmentDTO> EnrollAsync(CreateEnrollmentDirectRequest request);
}
