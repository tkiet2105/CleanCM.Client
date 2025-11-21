using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.Shared.Comments.Requests;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Images.Requests;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces;

public interface IImageClient
{
    Task<ApiResult<List<ImageDto>>> GetByProductAsync(Guid productId, CancellationToken token = default);
    Task<ApiResult<ImageDto?>> GetPrimaryAsync(Guid productId, CancellationToken token = default);

    Task<ApiResult<Guid>> CreateAsync(CreateImageRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> UpdateAsync(UpdateImageRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken token = default);
}
