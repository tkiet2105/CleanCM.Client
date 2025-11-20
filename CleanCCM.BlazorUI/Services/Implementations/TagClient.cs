using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class TagClient : BaseApiClient, ITagClient
{
    public TagClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<List<TagDto>>> GetAll(CancellationToken cancellationToken = default)
        => GetAsync<List<TagDto>>("api/tags/get-all", cancellationToken);
}
