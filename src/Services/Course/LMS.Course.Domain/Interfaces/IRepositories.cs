using LMS.Course.Domain.Entities;
using LMS.Shared.Contracts.Common;

namespace LMS.Course.Domain.Interfaces;

public interface ICourseRepository
{
    Task<PagedResult<CourseEntity>> GetAllAsync(int page, int size);
    Task<CourseEntity?> GetByIdAsync(int id);
    Task AddAsync(CourseEntity course);
    Task UpdateAsync(CourseEntity course);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}

public interface ISemesterRepository
{
    Task<PagedResult<Semester>> GetAllAsync(int page, int size);
    Task<Semester?> GetByIdAsync(int id);
    Task AddAsync(Semester semester);
    Task SaveChangesAsync();
}

public interface ISubjectRepository
{
    Task<PagedResult<Subject>> GetAllAsync(int page, int size);
    Task<Subject?> GetByIdAsync(int id);
    Task AddAsync(Subject subject);
    Task SaveChangesAsync();
}

public interface IEnrollmentRepository
{
    Task<PagedResult<Enrollment>> GetAllByCourseIdAsync(int courseId, int page, int size);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int studentId, int courseId);
    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync();
}
