

namespace CleanCCM.Shared.Categories.Requests;

public class UpdateCategoryRequest
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public int DisplayOrder { get; init; } = 0;
    public bool IsActive { get; init; } = true;
}