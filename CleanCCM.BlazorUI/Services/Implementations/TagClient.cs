using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Tags.Requests;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class TagClient : BaseApiClient, ITagClient
{
    public TagClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<List<TagDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<List<TagDto>>("api/tags/get-all", null, cancellationToken);

    public Task<ApiResult<TagDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => GetAsync<TagDto>("api/tags/get-by-id", new { id }, cancellationToken);

    public Task<ApiResult<Guid>> CreateAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<Guid>("api/tags/create", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        UpdateTagRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/tags/update", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/tags/delete", new { id }, cancellationToken);
}