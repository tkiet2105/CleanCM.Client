using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Addresses.Requests;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Rating.Requests;
using CleanCCM.Shared.Rating.Responses;
using CleanCCM.Shared.Tags.Requests;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class AddressClient : BaseApiClient, IAddressClient
{
    public AddressClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<List<AddressDto>>> GetByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
        => GetAsync<List<AddressDto>>("api/addresses/by-user", new { userId }, cancellationToken);

    public Task<ApiResult<AddressDto?>> GetPrimaryByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
        => GetAsync<AddressDto?>("api/addresses/primary-by-user", new { userId }, cancellationToken);

    public Task<ApiResult<AddressDto?>> GetByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
        => GetAsync<AddressDto?>("api/addresses/by-product", new { productId }, cancellationToken);

    public Task<ApiResult<Guid>> CreateAsync(
        CreateAddressRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<Guid>("api/addresses/create", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        UpdateAddressRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/addresses/update", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/addresses/delete", new { id }, cancellationToken);
}