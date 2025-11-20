
namespace CleanCCM.Shared.Products.Requests;

public class GetAllProductQueryRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? TagId { get; init; }
}
