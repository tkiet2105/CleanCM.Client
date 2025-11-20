using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces
{
    public interface IProductClient
    {
        Task<ApiResult<List<ProductDto>>> GetAll(GetAllProductQueryRequest query,CancellationToken token = default);

    }
}
