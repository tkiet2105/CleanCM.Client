

namespace CleanCCM.Shared.Images.Requests;


public class UpdateImageRequest
{
    public Guid Id { get; init; }

    public string? Alt { get; init; }
    public int? SortOrder { get; init; }
    public bool? IsPrimary { get; init; }

    public string? FileName { get; init; }
    public string? Url { get; init; }

    public Guid? ProductId { get; init; }
}