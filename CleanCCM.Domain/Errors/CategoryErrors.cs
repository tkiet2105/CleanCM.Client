using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Domain.Errors;

/// <summary>
/// Danh sách mã lỗi cho Category domain
/// 
/// MÃ LỖI: 3000-3099
/// </summary>
public static class CategoryErrors
{
    // ==================== NOT FOUND (3000-3019) ====================

    public static readonly ErrorCode NotFound =
        ErrorCode.Create(ErrorCategory.NotFound, 3001);

    public static readonly ErrorCode Deleted =
        ErrorCode.Create(ErrorCategory.NotFound, 3002);

    // ==================== VALIDATION (3020-3039) ====================

    public static readonly ErrorCode InvalidName =
        ErrorCode.Create(ErrorCategory.Validation, 3020);

    public static readonly ErrorCode InvalidSlug =
        ErrorCode.Create(ErrorCategory.Validation, 3021);

    // ==================== BUSINESS LOGIC (3040-3059) ====================

    /// <summary>
    /// Category đang có products, không thể xóa
    /// </summary>
    public static readonly ErrorCode HasProducts =
        ErrorCode.Create(ErrorCategory.Business, 3040);

    /// <summary>
    /// Category đang có subcategories, không thể xóa
    /// </summary>
    public static readonly ErrorCode HasSubCategories =
        ErrorCode.Create(ErrorCategory.Business, 3041);

    /// <summary>
    /// Parent category không tồn tại
    /// </summary>
    public static readonly ErrorCode ParentNotFound =
        ErrorCode.Create(ErrorCategory.NotFound, 3042);

    /// <summary>
    /// Không thể set category làm parent của chính nó
    /// </summary>
    public static readonly ErrorCode CannotBeSelfParent =
        ErrorCode.Create(ErrorCategory.Business, 3043);

    /// <summary>
    /// Slug đã tồn tại
    /// </summary>
    public static readonly ErrorCode DuplicateSlug =
        ErrorCode.Create(ErrorCategory.Conflict, 3060);
}
