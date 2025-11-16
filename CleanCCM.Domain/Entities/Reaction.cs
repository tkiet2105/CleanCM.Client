using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;


// Sử dụng: Reaction là cảm xúc của user đối với product (Like, Love, Haha...)
public class Reaction : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;
    public ReactionType Type { get; private set; }

    private Reaction() : base() { }

    public static Reaction Create(Guid productId, string userId, ReactionType type)
    {
        return new Reaction
        {
            ProductId = productId,
            UserId = userId,
            Type = type
        };
    }

    public void ChangeType(ReactionType newType)
    {
        Type = newType;
    }
}

public enum ReactionType
{
    Like = 1,
    Love = 2,
    Haha = 3,
    Wow = 4,
    Sad = 5,
    Angry = 6
}