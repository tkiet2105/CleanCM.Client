using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;
// ==================== IAddressRepository ====================
// Location: CleanCCM.Application/Common/Interfaces/IAddressRepository.cs

/// <summary>
/// Sử dụng: Repository cho Address với các query đặc thù
/// </summary>
public interface IAddressRepository : IRepository<Address>
{
    // ==================== USER ====================

    /// <summary>
    /// Lấy tất cả địa chỉ của user, ưu tiên địa chỉ chính lên đầu.
    /// </summary>
    Task<List<Address>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy địa chỉ chính của user (nếu có).
    /// </summary>
    Task<Address?> GetPrimaryByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra user đã có địa chỉ chính chưa.
    /// </summary>
    Task<bool> UserHasPrimaryAsync(string userId, CancellationToken cancellationToken = default);


    // ==================== PRODUCT ====================

    /// <summary>
    /// Lấy địa chỉ của product nếu product có gắn địa chỉ.
    /// </summary>
    Task<Address?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra product có địa chỉ không.
    /// </summary>
    Task<bool> ProductHasAddressAsync(Guid productId, CancellationToken cancellationToken = default);
}