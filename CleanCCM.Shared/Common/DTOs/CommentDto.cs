namespace CleanCCM.Shared.Common.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }

    public List<CommentDto> Replies { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
