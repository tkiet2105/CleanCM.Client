namespace CleanCCM.Domain.Common.Errors;

/// <summary>
/// Class đại diện cho MÃ LỖI trong hệ thống
/// 
/// ĐỌC HIỂU:
/// - Mỗi lỗi có 2 thành phần: Category (nhóm) + Code (mã số)
/// - Format cuối cùng: CATEGORY_CODE (ví dụ: AUTH_1001, VAL_2001)
/// - Sử dụng int cho Code để tránh trùng lặp và dễ quản lý
/// 
/// VÍ DỤ:
/// - ErrorCode.Create("AUTH", 1001) → Tạo mã lỗi AUTH_1001
/// - FullCode sẽ trả về chuỗi hoàn chỉnh: "AUTH_1001"
/// </summary>
public sealed record ErrorCode
{
    /// <summary>
    /// MÃ SỐ của lỗi (dạng số nguyên)
    /// Ví dụ: 1001, 2001, 3001
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// DANH MỤC của lỗi (từ ErrorCategory)
    /// Ví dụ: AUTH, VAL, NF
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// MÃ LỖI HOÀN CHỈNH dưới dạng chuỗi
    /// Format: CATEGORY_CODE
    /// Ví dụ: AUTH_1001, VAL_2001
    /// </summary>
    public string FullCode => $"{Category}_{Code}";

    /// <summary>
    /// Constructor PRIVATE - Chỉ tạo ErrorCode qua method Create()
    /// Điều này đảm bảo tất cả ErrorCode được tạo theo chuẩn
    /// </summary>
    private ErrorCode(string category, int code)
    {
        Category = category;
        Code = code;
    }

    /// <summary>
    /// Tạo một ErrorCode mới
    /// 
    /// CÁCH DÙNG:
    /// var errorCode = ErrorCode.Create(ErrorCategory.Authentication, 1001);
    /// Kết quả: errorCode.FullCode = "AUTH_1001"
    /// </summary>
    /// <param name="category">Danh mục lỗi (AUTH, VAL, NF...)</param>
    /// <param name="code">Mã số lỗi (1001, 2001...)</param>
    /// <returns>ErrorCode object</returns>
    public static ErrorCode Create(string category, int code) => new(category, code);

    /// <summary>
    /// Chuyển ErrorCode thành chuỗi để hiển thị
    /// Kết quả trả về: CATEGORY_CODE
    /// </summary>
    public override string ToString() => FullCode;
}