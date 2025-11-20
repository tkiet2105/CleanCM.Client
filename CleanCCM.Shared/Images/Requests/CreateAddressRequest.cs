

namespace CleanCCM.Shared.Images.Requests;


public class CreateImageRequest
{
    public Guid ProductId { get; init; }

    public string FileName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;

    public string? Alt { get; init; }
    public int SortOrder { get; init; } = 0;
    public bool IsPrimary { get; init; } = false;
}