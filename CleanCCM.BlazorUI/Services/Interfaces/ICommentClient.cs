using CleanCCM.Shared.Comments.Requests;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces;

public interface ICommentClient
{
    Task<ApiResult<PaginatedList<CommentDto>>> GetByProductAsync(GetCommentsByProductRequest request, CancellationToken token = default);
    Task<ApiResult<List<CommentDto>>> GetByUserAsync(string userId, CancellationToken token = default);
    Task<ApiResult<List<CommentDto>>> GetRepliesAsync(Guid parentCommentId, CancellationToken token = default);
    Task<ApiResult<int>> GetCountByProductAsync(Guid productId, CancellationToken token = default);

    Task<ApiResult<Guid>> CreateAsync(CreateCommentRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> UpdateAsync(UpdateCommentRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken token = default);
}
