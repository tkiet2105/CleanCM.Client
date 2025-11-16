

namespace CleanCCM.Domain.Enums;

public static class OrderStatus
{
    public const string pending = nameof(pending);
    public const string confirmed = nameof(confirmed);
    public const string processing = nameof(processing);
    public const string shipping = nameof(shipping);
    public const string delivered = nameof(delivered);
    public const string cancelled = nameof(cancelled);
    public const string returned = nameof(returned);
    public const string refunding = nameof(refunding);
    public const string refunded = nameof(refunded);
}

