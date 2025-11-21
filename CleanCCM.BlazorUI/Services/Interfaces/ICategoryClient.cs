using CleanCCM.Shared.Categories.Requests;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces;

public interface ICategoryClient
{
    Task<ApiResult<List<CategoryDto>>> GetAllAsync(CancellationToken token = default);
    Task<ApiResult<List<CategoryDto>>> GetActiveAsync(CancellationToken token = default);
    Task<ApiResult<CategoryDto>> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<ApiResult<Guid>> CreateAsync(CreateCategoryRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> UpdateAsync(UpdateCategoryRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken token = default);
}
