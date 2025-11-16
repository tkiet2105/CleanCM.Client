namespace CleanCCM.Application.Features.Ratings.DTOs;

public class RatingDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
}