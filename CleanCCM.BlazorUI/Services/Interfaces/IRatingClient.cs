using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;
using CleanCCM.Shared.Rating.Requests;
using CleanCCM.Shared.Rating.Responses;

namespace CleanCCM.BlazorUI.Services.Interfaces;
public interface IRatingClient
{
    Task<ApiResult<PaginatedList<RatingDto>>> GetByProductAsync(GetRatingsByProductRequest request, CancellationToken token = default);
    Task<ApiResult<RatingDto?>> GetMyRatingAsync(Guid productId, CancellationToken token = default);
    Task<ApiResult<RatingSummaryDto>> GetSummaryAsync(Guid productId, CancellationToken token = default);

    Task<ApiResult<Guid>> CreateAsync(CreateRatingRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> UpdateAsync(UpdateRatingRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken token = default);
}
