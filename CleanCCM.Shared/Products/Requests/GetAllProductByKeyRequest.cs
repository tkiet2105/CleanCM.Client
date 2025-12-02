
namespace CleanCCM.Shared.Products.Requests;

public class GetProductsByKeyRequest
{
    /// <summary>
    /// Danh sách category slug/key (OR). Nếu null/rỗng → không lọc theo category.
    /// </summary>
    public List<string>? CategoryKeys { get; init; }

    /// <summary>
    /// Danh sách tag slug/key (OR). Nếu null/rỗng → không lọc theo tag.
    /// </summary>
    public List<string>? TagKeys { get; init; }

    /// <summary>
    /// Danh sách ward key (tên phường/xã). Nếu null/rỗng → không lọc theo ward.
    /// </summary>
    public List<string>? WardKeys { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}