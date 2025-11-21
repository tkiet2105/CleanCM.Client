namespace CleanCCM.Application.Features.Ratings.DTOs;

public class RatingSummaryDto
{
    public double AverageScore { get; set; }
    public int TotalRatings { get; set; }
    public Dictionary<int, int> Distribution { get; set; } = new(); // key: score, value: count
}
