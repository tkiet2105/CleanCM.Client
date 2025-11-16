using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Domain.Errors;


/// <summary>
/// Danh sách mã lỗi cho Tag domain
/// 
/// MÃ LỖI: 3100-3199
/// </summary>
public static class TagErrors
{
    // ==================== NOT FOUND (3100-3119) ====================

    public static readonly ErrorCode NotFound =
        ErrorCode.Create(ErrorCategory.NotFound, 3101);

    public static readonly ErrorCode Deleted =
        ErrorCode.Create(ErrorCategory.NotFound, 3102);

    // ==================== VALIDATION (3120-3139) ====================

    public static readonly ErrorCode InvalidName =
        ErrorCode.Create(ErrorCategory.Validation, 3120);

    public static readonly ErrorCode InvalidSlug =
        ErrorCode.Create(ErrorCategory.Validation, 3121);

    // ==================== BUSINESS LOGIC (3140-3159) ====================

    /// <summary>
    /// Tag đang được sử dụng bởi products, không thể xóa
    /// </summary>
    public static readonly ErrorCode InUse =
        ErrorCode.Create(ErrorCategory.Business, 3140);

    /// <summary>
    /// Slug đã tồn tại
    /// </summary>
    public static readonly ErrorCode DuplicateSlug =
        ErrorCode.Create(ErrorCategory.Conflict, 3160);

    /// <summary>
    /// Tên tag đã tồn tại
    /// </summary>
    public static readonly ErrorCode DuplicateName =
        ErrorCode.Create(ErrorCategory.Conflict, 3161);
}