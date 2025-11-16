namespace CleanCCM.Application.Common.Models;


public class PaginationRequest
{
    private int _pageNumber = 1;

    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    private int _pageSize = DefaultPageSize;

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value <= 0)
            {
                _pageSize = DefaultPageSize;
            }
            else if (value > MaxPageSize)
            {
                _pageSize = MaxPageSize;
            }
            else
            {
                _pageSize = value;
            }
        }
    }

    public string? SearchTerm { get; set; }

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;

    public int Skip => (PageNumber - 1) * PageSize;

    public int Take => PageSize;
}
