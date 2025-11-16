using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;


// Sử dụng: Rating là đánh giá sao và review của user cho product
public class Rating : BaseAuditableEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;
    public int Score { get; private set; }
    public string? Review { get; private set; }

    private Rating() : base() { }

    public static Rating Create(Guid productId, string userId, int score, string? review = null)
    {
        if (score < 1 || score > 5)
            throw new ArgumentException("Score must be between 1 and 5", nameof(score));

        return new Rating
        {
            ProductId = productId,
            UserId = userId,
            Score = score,
            Review = review
        };
    }

    public void Update(int score, string? review)
    {
        if (score < 1 || score > 5)
            throw new ArgumentException("Score must be between 1 and 5", nameof(score));

        Score = score;
        Review = review;
    }
}


