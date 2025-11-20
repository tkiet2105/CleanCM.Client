using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces
{
    public interface ITagClient
    {
        /// <summary>
        /// Lấy toàn bộ danh sách tags 1 lần duy nhất
        /// </summary>
        /// <returns></returns>
        Task<ApiResult<List<TagDto>>> GetAll(CancellationToken cancellationToken = default);



    }
}
