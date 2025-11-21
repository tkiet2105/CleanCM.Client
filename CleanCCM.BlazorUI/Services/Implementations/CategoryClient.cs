using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Categories.Requests;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class CategoryClient : BaseApiClient, ICategoryClient
{
    public CategoryClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<List<CategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<List<CategoryDto>>("api/categories/get-all", null, cancellationToken);

    public Task<ApiResult<List<CategoryDto>>> GetActiveAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<List<CategoryDto>>("api/categories/get-active", null, cancellationToken);

    public Task<ApiResult<CategoryDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => GetAsync<CategoryDto>("api/categories/get-by-id", new { id }, cancellationToken);

    public Task<ApiResult<Guid>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<Guid>("api/categories/create", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/categories/update", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/categories/delete", new { id }, cancellationToken);
}