

namespace CleanCCM.Shared.Comments.Requests;


public class UpdateCommentRequest
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
}
