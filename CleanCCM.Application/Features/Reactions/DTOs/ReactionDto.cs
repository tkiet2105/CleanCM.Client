namespace CleanCCM.Application.Features.Reactions.DTOs;

public class ReactionDto
{
    public Guid ProductId { get; set; }
    public int TotalCount { get; set; }
    public int LikeCount { get; set; }
    public int LoveCount { get; set; }
    public int HahaCount { get; set; }
    public int WowCount { get; set; }
    public int SadCount { get; set; }
    public int AngryCount { get; set; }
}