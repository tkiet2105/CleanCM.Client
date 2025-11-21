using CleanCCM.BlazorClient.ApiClients;
using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Comments.Requests;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class CommentClient : BaseApiClient, ICommentClient
{
    public CommentClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<PaginatedList<CommentDto>>> GetByProductAsync(
        GetCommentsByProductRequest request,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedList<CommentDto>>("api/comments/by-product", request, cancellationToken);

    public Task<ApiResult<List<CommentDto>>> GetByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
        => GetAsync<List<CommentDto>>("api/comments/by-user", new { userId }, cancellationToken);

    public Task<ApiResult<List<CommentDto>>> GetRepliesAsync(
        Guid parentCommentId,
        CancellationToken cancellationToken = default)
        => GetAsync<List<CommentDto>>("api/comments/replies", new { parentCommentId }, cancellationToken);

    public Task<ApiResult<int>> GetCountByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
        => GetAsync<int>("api/comments/count", new { productId }, cancellationToken);

    public Task<ApiResult<Guid>> CreateAsync(
        CreateCommentRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<Guid>("api/comments/create", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        UpdateCommentRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/comments/update", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => PostAsync<bool>("api/comments/delete", new { id }, cancellationToken);
}