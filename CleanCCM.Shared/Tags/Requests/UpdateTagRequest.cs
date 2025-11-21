
namespace CleanCCM.Shared.Tags.Requests;

public class UpdateTagRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}
