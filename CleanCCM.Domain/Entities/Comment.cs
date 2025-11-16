using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;

// Sử dụng: Comment là bình luận của user về product, có thể reply lẫn nhau
public class Comment : BaseAuditableEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;

    // Self-referencing for replies
    public Guid? ParentCommentId { get; private set; }
    public Comment? ParentComment { get; private set; }
    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

    private Comment() : base() { }

    public static Comment Create(Guid productId, string userId, string content, Guid? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        return new Comment
        {
            ProductId = productId,
            UserId = userId,
            Content = content,
            ParentCommentId = parentCommentId
        };
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        Content = content;
    }
}


