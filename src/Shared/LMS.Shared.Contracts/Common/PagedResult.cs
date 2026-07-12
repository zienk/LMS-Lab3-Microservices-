namespace LMS.Shared.Contracts.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public PaginationMeta Pagination { get; set; } = new();
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
