namespace CleanCCM.Shared.Common.DTOs;

public class ImageDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Alt { get; set; }

    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }
}
