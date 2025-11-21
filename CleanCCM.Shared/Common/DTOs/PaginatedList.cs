namespace CleanCCM.Shared.Common.DTOs;


public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // Ctor rỗng cho serializer
    public PaginatedList() { }

    public PaginatedList(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items.ToList();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
        => new(Array.Empty<T>(), 0, pageNumber, pageSize);
}
