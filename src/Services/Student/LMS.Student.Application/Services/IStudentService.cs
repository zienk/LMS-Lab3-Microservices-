using LMS.Student.Application.DTOs;
using LMS.Shared.Contracts.Common;

namespace LMS.Student.Application.Services;

public interface IStudentService
{
    Task<PagedResult<StudentResponse>> GetAllAsync(int page, int size, string? search, string? sort);
    Task<StudentResponse?> GetByIdAsync(int id);
    Task<StudentResponse> CreateAsync(StudentRequest request);
    Task<StudentResponse> UpdateAsync(int id, StudentRequest request);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
