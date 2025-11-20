
namespace CleanCCM.Shared.Products.Requests;

public class CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }

    public List<Guid> CategoryIds { get; init; } = new();
    public List<Guid> TagIds { get; init; } = new();
}