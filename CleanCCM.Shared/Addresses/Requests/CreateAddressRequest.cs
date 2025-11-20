

namespace CleanCCM.Shared.Addresses.Requests;


public class CreateAddressRequest
{
    public string Line1 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Ward { get; init; } = string.Empty;

    public string? Line2 { get; init; }
    public string Country { get; init; } = "Vietnam";

    public bool IsPrimary { get; init; }

    public Guid? ProductId { get; init; }
    public string? UserId { get; init; }
}