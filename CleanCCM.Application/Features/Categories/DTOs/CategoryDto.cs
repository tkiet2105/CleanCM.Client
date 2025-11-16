namespace CleanCCM.Application.Features.Categories.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
}