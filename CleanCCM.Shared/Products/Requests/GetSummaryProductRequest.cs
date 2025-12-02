
namespace CleanCCM.Shared.Products.Requests;

public class GetSummaryProductRequest
{
    public List<string>? CategoryKeys { get; set; }
    public List<string>? TagKeys { get; set; }
    public List<string>? WardKeys { get; set; }
}