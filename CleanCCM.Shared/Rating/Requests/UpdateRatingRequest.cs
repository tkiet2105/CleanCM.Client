
namespace CleanCCM.Shared.Rating.Responses;

public class UpdateRatingRequest
{
    public Guid Id { get; init; }
    public int Score { get; init; }
    public string? Review { get; init; }
}