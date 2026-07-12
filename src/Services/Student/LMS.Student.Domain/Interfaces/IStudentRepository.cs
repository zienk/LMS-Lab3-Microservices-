using LMS.Student.Domain.Entities;
using LMS.Shared.Contracts.Common;

namespace LMS.Student.Domain.Interfaces;

public interface IStudentRepository
{
    Task<PagedResult<Domain.Entities.Student>> GetAllAsync(int page, int size, string? search, string? sort);
    Task<Domain.Entities.Student?> GetByIdAsync(int id);
    Task<Domain.Entities.Student?> GetByCodeAsync(string code);
    Task AddAsync(Domain.Entities.Student student);
    Task UpdateAsync(Domain.Entities.Student student);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}
