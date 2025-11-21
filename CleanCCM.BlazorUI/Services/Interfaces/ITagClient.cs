using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;
using CleanCCM.Shared.Tags.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces
{
    public interface ITagClient
    {
        Task<ApiResult<List<TagDto>>> GetAllAsync(CancellationToken token = default);
        Task<ApiResult<TagDto>> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<ApiResult<Guid>> CreateAsync(CreateTagRequest request, CancellationToken token = default);
        Task<ApiResult<bool>> UpdateAsync(UpdateTagRequest request, CancellationToken token = default);
        Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken token = default);
    }
}
