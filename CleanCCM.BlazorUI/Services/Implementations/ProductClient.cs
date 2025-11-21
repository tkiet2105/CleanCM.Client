using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class ProductClient : BaseApiClient, IProductClient
{
    public ProductClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<PaginatedList<ProductDto>>> GetAllAsync(
        GetAllProductRequest request,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedList<ProductDto>>("api/products/get-all", request, cancellationToken);

    public Task<ApiResult<ProductDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => GetAsync<ProductDto>("api/products/get-by-id", new { id }, cancellationToken);

    public Task<ApiResult<Guid>> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<Guid>("api/products/create", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/products/update", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/products/delete", new { id }, cancellationToken);
}