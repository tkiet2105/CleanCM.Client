using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Domain.Errors;

/// <summary>
/// Danh sách mã lỗi cho Product domain
/// 
/// QUY TẮC MÃ LỖI:
/// - Product errors: 2000-2999
/// - Chia nhỏ theo loại:
///   + 2000-2099: Not Found
///   + 2100-2199: Validation
///   + 2200-2299: Business Logic
///   + 2300-2399: Engagement (Like, Comment...)
/// </summary>
public static class ProductErrors
{
    // ==================== NOT FOUND (2000-2099) ====================

    /// <summary>
    /// Product không tồn tại
    /// </summary>
    public static readonly ErrorCode NotFound =
        ErrorCode.Create(ErrorCategory.NotFound, 2001);

    /// <summary>
    /// Product đã bị xóa (soft delete)
    /// </summary>
    public static readonly ErrorCode Deleted =
        ErrorCode.Create(ErrorCategory.NotFound, 2002);

    // ==================== VALIDATION (2100-2199) ====================

    /// <summary>
    /// Tên product không hợp lệ
    /// </summary>
    public static readonly ErrorCode InvalidName =
        ErrorCode.Create(ErrorCategory.Validation, 2101);

    /// <summary>
    /// Giá không hợp lệ (âm hoặc 0)
    /// </summary>
    public static readonly ErrorCode InvalidPrice =
        ErrorCode.Create(ErrorCategory.Validation, 2102);

    /// <summary>
    /// Giá giảm giá không hợp lệ (> giá gốc)
    /// </summary>
    public static readonly ErrorCode InvalidDiscountPrice =
        ErrorCode.Create(ErrorCategory.Validation, 2103);

    /// <summary>
    /// Số lượng tồn kho không hợp lệ (âm)
    /// </summary>
    public static readonly ErrorCode InvalidStock =
        ErrorCode.Create(ErrorCategory.Validation, 2104);

    // ==================== BUSINESS LOGIC (2200-2299) ====================

    /// <summary>
    /// Product đã inactive, không thể thao tác
    /// </summary>
    public static readonly ErrorCode Inactive =
        ErrorCode.Create(ErrorCategory.Business, 2201);

    /// <summary>
    /// Product hết hàng
    /// </summary>
    public static readonly ErrorCode OutOfStock =
        ErrorCode.Create(ErrorCategory.Business, 2202);

    /// <summary>
    /// Product không có category
    /// </summary>
    public static readonly ErrorCode NoCategory =
        ErrorCode.Create(ErrorCategory.Business, 2203);

    /// <summary>
    /// SKU đã tồn tại
    /// </summary>
    public static readonly ErrorCode DuplicateSKU =
        ErrorCode.Create(ErrorCategory.Conflict, 2301);

    // ==================== ENGAGEMENT (2300-2399) ====================

    /// <summary>
    /// User đã like product này rồi
    /// </summary>
    public static readonly ErrorCode AlreadyLiked =
        ErrorCode.Create(ErrorCategory.Conflict, 2301);

    /// <summary>
    /// User chưa like product này
    /// </summary>
    public static readonly ErrorCode NotLiked =
        ErrorCode.Create(ErrorCategory.Business, 2302);

    /// <summary>
    /// User đã dislike product này rồi
    /// </summary>
    public static readonly ErrorCode AlreadyDisliked =
        ErrorCode.Create(ErrorCategory.Conflict, 2303);

    /// <summary>
    /// User chưa dislike product này
    /// </summary>
    public static readonly ErrorCode NotDisliked =
        ErrorCode.Create(ErrorCategory.Business, 2304);

    /// <summary>
    /// User đã rate product này rồi
    /// </summary>
    public static readonly ErrorCode AlreadyRated =
        ErrorCode.Create(ErrorCategory.Conflict, 2305);

    /// <summary>
    /// Rating value không hợp lệ (phải từ 1-5)
    /// </summary>
    public static readonly ErrorCode InvalidRatingValue =
        ErrorCode.Create(ErrorCategory.Validation, 2306);
}