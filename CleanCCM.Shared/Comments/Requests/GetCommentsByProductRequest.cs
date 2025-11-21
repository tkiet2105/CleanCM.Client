
namespace CleanCCM.Shared.Comments.Requests;
public class GetCommentsByProductRequest
{
    public Guid ProductId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
