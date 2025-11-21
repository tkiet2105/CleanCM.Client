namespace CleanCCM.Shared.Rating.Requests;

public class CreateRatingRequest
{
    public Guid ProductId { get; init; }
    public int Score { get; init; }
    public string? Review { get; init; }
}