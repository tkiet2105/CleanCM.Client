namespace CleanCCM.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; }

    public int PageNumber { get; }

    public int TotalPages { get; }

    public int TotalCount { get; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;

        // TÍNH TỔNG SỐ TRANG
        // Ceiling: Làm tròn lên
        // Ví dụ: 95/10 = 9.5 → Ceiling = 10 trang
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    public static PaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        // COUNT tổng số items
        var count = source.Count();

        // SKIP + TAKE để lấy items của trang hiện tại
        // Skip: Bỏ qua (pageNumber - 1) * pageSize items
        // Take: Lấy pageSize items tiếp theo
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}