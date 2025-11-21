
namespace CleanCCM.Shared.Comments.Requests;

public class CreateCommentRequest
{
    public Guid ProductId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid? ParentCommentId { get; init; }
}