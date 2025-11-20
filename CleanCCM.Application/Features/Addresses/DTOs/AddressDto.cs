namespace CleanCCM.Application.Addresses.DTOs;

public class AddressDto
{
    public Guid Id { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string Country { get; set; } = "Vietnam";
    public bool IsPrimary { get; set; }
    public Guid? ProductId { get; set; }
    public string? UserId { get; set; }
}
