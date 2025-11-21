using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Rating.Requests;
using CleanCCM.Shared.Rating.Responses;
using CleanCCM.Shared.Tags.Requests;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class RatingClient : BaseApiClient, IRatingClient
{
    public RatingClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<PaginatedList<RatingDto>>> GetByProductAsync(
        GetRatingsByProductRequest request,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedList<RatingDto>>("api/ratings/by-product", request, cancellationToken);

    public Task<ApiResult<RatingDto?>> GetMyRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
        => GetAsync<RatingDto?>("api/ratings/my-rating", new { productId }, cancellationToken);

    public Task<ApiResult<RatingSummaryDto>> GetSummaryAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
        => GetAsync<RatingSummaryDto>("api/ratings/summary", new { productId }, cancellationToken);

    public Task<ApiResult<Guid>> CreateAsync(
        CreateRatingRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<Guid>("api/ratings/create", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        UpdateRatingRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/ratings/update", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/ratings/delete", new { id }, cancellationToken);
}