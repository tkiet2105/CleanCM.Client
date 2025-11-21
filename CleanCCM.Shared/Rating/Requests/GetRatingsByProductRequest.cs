
namespace CleanCCM.Shared.Rating.Responses;

public class GetRatingsByProductRequest
{
    public Guid ProductId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}