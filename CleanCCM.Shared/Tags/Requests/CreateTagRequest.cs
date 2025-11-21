

namespace CleanCCM.Shared.Tags.Requests;

public class CreateTagRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}
