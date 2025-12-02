
namespace CleanCCM.Application.Features.Products.DTOs;


public class ProductSummaryDto
{
    public int TotalProducts { get; set; }

    public List<CategorySummaryDto> Categories { get; set; } = new();
    public List<TagSummaryDto> Tags { get; set; } = new();
    public List<WardSummaryDto> Wards { get; set; } = new();
}

public class CategorySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Slug { get; set; }
    public int ProductCount { get; set; }
}

public class TagSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Slug { get; set; }
    public int ProductCount { get; set; }
}

public class WardSummaryDto
{
    public string WardKey { get; set; } = default!;
    public int ProductCount { get; set; }
}