using LMS.Student.Application.DTOs;
using LMS.Student.Application.Services;
using LMS.Student.Domain.Entities;
using LMS.Student.Domain.Interfaces;
using LMS.Shared.Contracts.Common;

namespace LMS.Student.Infrastructure.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo) => _repo = repo;

    public async Task<PagedResult<StudentResponse>> GetAllAsync(int page, int size, string? search, string? sort)
    {
        var result = await _repo.GetAllAsync(page, size, search, sort);
        return new PagedResult<StudentResponse>
        {
            Data = result.Data.Select(Map),
            Pagination = result.Pagination
        };
    }

    public async Task<StudentResponse?> GetByIdAsync(int id)
    {
        var s = await _repo.GetByIdAsync(id);
        return s is null ? null : Map(s);
    }

    public async Task<StudentResponse> CreateAsync(StudentRequest request)
    {
        var student = new Student.Domain.Entities.Student
        {
            FullName = request.FullName,
            Email = request.Email,
            StudentCode = request.StudentCode,
            DateOfBirth = request.DateOfBirth
        };
        await _repo.AddAsync(student);
        await _repo.SaveChangesAsync();
        return Map(student);
    }

    public async Task<StudentResponse> UpdateAsync(int id, StudentRequest request)
    {
        var student = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Student {id} not found.");

        student.FullName = request.FullName;
        student.Email = request.Email;
        student.StudentCode = request.StudentCode;
        student.DateOfBirth = request.DateOfBirth;

        await _repo.UpdateAsync(student);
        await _repo.SaveChangesAsync();
        return Map(student);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _repo.ExistsAsync(id))
            throw new KeyNotFoundException($"Student {id} not found.");
        await _repo.DeleteAsync(id);
        await _repo.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id) => _repo.ExistsAsync(id);

    private static StudentResponse Map(Domain.Entities.Student s) => new()
    {
        StudentId = s.StudentId,
        FullName = s.FullName,
        Email = s.Email,
        StudentCode = s.StudentCode,
        DateOfBirth = s.DateOfBirth,
        IsActive = s.IsActive
    };
}
