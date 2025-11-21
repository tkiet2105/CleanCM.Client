using CleanCCM.Shared.Addresses.Requests;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Interfaces;

public interface IAddressClient
{
    Task<ApiResult<List<AddressDto>>> GetByUserAsync(string userId, CancellationToken token = default);
    Task<ApiResult<AddressDto?>> GetPrimaryByUserAsync(string userId, CancellationToken token = default);
    Task<ApiResult<AddressDto?>> GetByProductAsync(Guid productId, CancellationToken token = default);

    Task<ApiResult<Guid>> CreateAsync(CreateAddressRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> UpdateAsync(UpdateAddressRequest request, CancellationToken token = default);
    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken token = default);
}
