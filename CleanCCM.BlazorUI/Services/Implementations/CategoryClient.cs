using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class CategoryClient : BaseApiClient, ICategoryClient
{
    public CategoryClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<List<CategoryDto>>> GetAll(CancellationToken cancellationToken = default)
        => GetAsync<List<CategoryDto>>("api/categories/get-all", cancellationToken);
}
