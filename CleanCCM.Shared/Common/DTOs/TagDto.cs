namespace CleanCCM.Shared.Common.DTOs;

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
}