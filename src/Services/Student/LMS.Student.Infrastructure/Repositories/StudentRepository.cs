using LMS.Student.Domain.Entities;
using LMS.Student.Domain.Interfaces;
using LMS.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context;

    public StudentRepository(StudentDbContext context) => _context = context;

    public async Task<PagedResult<Domain.Entities.Student>> GetAllAsync(int page, int size, string? search, string? sort)
    {
        var query = _context.Students.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.FullName.Contains(search) || s.Email.Contains(search) || s.StudentCode.Contains(search));

        query = sort?.ToLower() switch
        {
            "fullname" => query.OrderBy(s => s.FullName),
            "-fullname" => query.OrderByDescending(s => s.FullName),
            "email" => query.OrderBy(s => s.Email),
            _ => query.OrderBy(s => s.StudentId)
        };

        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        return new PagedResult<Domain.Entities.Student>
        {
            Data = data,
            Pagination = new PaginationMeta
            {
                Page = page,
                PageSize = size,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling((double)total / size)
            }
        };
    }

    public async Task<Domain.Entities.Student?> GetByIdAsync(int id)
        => await _context.Students.FindAsync(id);

    public async Task<Domain.Entities.Student?> GetByCodeAsync(string code)
        => await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == code);

    public async Task AddAsync(Domain.Entities.Student student)
        => await _context.Students.AddAsync(student);

    public async Task UpdateAsync(Domain.Entities.Student student)
        => _context.Students.Update(student);

    public async Task DeleteAsync(int id)
    {
        var s = await _context.Students.FindAsync(id);
        if (s != null) _context.Students.Remove(s);
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Students.AnyAsync(s => s.StudentId == id);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
