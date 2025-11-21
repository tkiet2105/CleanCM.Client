namespace CleanCCM.Shared.Common.DTOs;

public class RatingSummaryDto
{
    public double AverageScore { get; set; }
    public int TotalRatings { get; set; }

    // key: score (1-5), value: count
    public Dictionary<int, int> Distribution { get; set; } = new();
}
