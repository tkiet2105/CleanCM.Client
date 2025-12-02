using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces
{
    public interface IProductClient
    {
        Task<ApiResult<ProductSummaryDto>> GetSummaryAsync(GetSummaryProductRequest request, CancellationToken cancellationToken = default);
        Task<ApiResult<PaginatedList<ProductDto>>> GetAllAsync(GetAllProductRequest request, CancellationToken token = default);
        Task<ApiResult<PaginatedList<ProductDto>>> FilterByKeysAsync(GetProductsByKeyRequest request, CancellationToken token = default);
        Task<ApiResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<ApiResult<Guid>> CreateAsync(CreateProductRequest request, CancellationToken token = default);
        Task<ApiResult<bool>> UpdateAsync(UpdateProductRequest request, CancellationToken token = default);
        Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken token = default);
    }
}
