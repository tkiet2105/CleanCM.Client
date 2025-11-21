using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Images.Requests;
using CleanCCM.Shared.Rating.Requests;
using CleanCCM.Shared.Rating.Responses;
using CleanCCM.Shared.Tags.Requests;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class ImageClient : BaseApiClient, IImageClient
{
    public ImageClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<List<ImageDto>>> GetByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
        => GetAsync<List<ImageDto>>("api/images/by-product", new { productId }, cancellationToken);

    public Task<ApiResult<ImageDto?>> GetPrimaryAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
        => GetAsync<ImageDto?>("api/images/primary", new { productId }, cancellationToken);

    public Task<ApiResult<Guid>> CreateAsync(
        CreateImageRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<Guid>("api/images/create", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        UpdateImageRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/images/update", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/images/delete", new { id }, cancellationToken);
}