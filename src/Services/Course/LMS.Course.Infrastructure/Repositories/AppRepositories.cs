using LMS.Course.Domain.Entities;
using LMS.Course.Domain.Interfaces;
using LMS.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS.Course.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly CourseDbContext _context;
    public CourseRepository(CourseDbContext context) => _context = context;

    public async Task<PagedResult<CourseEntity>> GetAllAsync(int page, int size)
    {
        var total = await _context.Courses.CountAsync();
        var data = await _context.Courses.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<CourseEntity> { Data = data, Pagination = new PaginationMeta { Page = page, PageSize = size, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)size) } };
    }

    public async Task<CourseEntity?> GetByIdAsync(int id) => await _context.Courses.FindAsync(id);
    public async Task AddAsync(CourseEntity course) => await _context.Courses.AddAsync(course);
    public Task UpdateAsync(CourseEntity course)
    {
        _context.Courses.Update(course);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is not null)
            _context.Courses.Remove(course);
    }

    public async Task<bool> ExistsAsync(int id) => await _context.Courses.AnyAsync(c => c.CourseId == id);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}

public class SemesterRepository : ISemesterRepository
{
    private readonly CourseDbContext _context;
    public SemesterRepository(CourseDbContext context) => _context = context;

    public async Task<PagedResult<Semester>> GetAllAsync(int page, int size)
    {
        var total = await _context.Semesters.CountAsync();
        var data = await _context.Semesters.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Semester> { Data = data, Pagination = new PaginationMeta { Page = page, PageSize = size, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)size) } };
    }
    public async Task<Semester?> GetByIdAsync(int id) => await _context.Semesters.FindAsync(id);
    public async Task AddAsync(Semester semester) => await _context.Semesters.AddAsync(semester);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}

public class SubjectRepository : ISubjectRepository
{
    private readonly CourseDbContext _context;
    public SubjectRepository(CourseDbContext context) => _context = context;

    public async Task<PagedResult<Subject>> GetAllAsync(int page, int size)
    {
        var total = await _context.Subjects.CountAsync();
        var data = await _context.Subjects.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Subject> { Data = data, Pagination = new PaginationMeta { Page = page, PageSize = size, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)size) } };
    }
    public async Task<Subject?> GetByIdAsync(int id) => await _context.Subjects.FindAsync(id);
    public async Task AddAsync(Subject subject) => await _context.Subjects.AddAsync(subject);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly CourseDbContext _context;
    public EnrollmentRepository(CourseDbContext context) => _context = context;

    public async Task<PagedResult<Enrollment>> GetAllByCourseIdAsync(int courseId, int page, int size)
    {
        var query = _context.Enrollments.Where(e => e.CourseId == courseId);
        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Enrollment> { Data = data, Pagination = new PaginationMeta { Page = page, PageSize = size, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)size) } };
    }

    public async Task<Enrollment?> GetByIdAsync(int id) => await _context.Enrollments.FindAsync(id);
    public async Task<bool> ExistsAsync(int studentId, int courseId) => await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    public async Task AddAsync(Enrollment enrollment) => await _context.Enrollments.AddAsync(enrollment);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
