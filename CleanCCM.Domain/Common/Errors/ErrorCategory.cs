namespace CleanCCM.Domain.Common.Errors;

/// <summary>
/// Định nghĩa các DANH MỤC lỗi trong hệ thống
/// Mỗi category đại diện cho một NHÓM lỗi cụ thể
/// 
/// ĐỌC HIỂU:
/// - Các category này giống như "folder" để phân loại lỗi
/// - Ví dụ: Tất cả lỗi liên quan đến xác thực sẽ thuộc AUTH
/// - Giúp dễ dàng tìm kiếm và quản lý lỗi theo nhóm
/// </summary>
public static class ErrorCategory
{
    /// <summary>
    /// Nhóm lỗi XÁC THỰC (Authentication)
    /// - Dùng cho: Đăng nhập, đăng ký, JWT token
    /// - Ví dụ: Sai mật khẩu, token hết hạn
    /// </summary>
    public const string Authentication = "AUTH";

    /// <summary>
    /// Nhóm lỗi PHÂN QUYỀN (Authorization)
    /// - Dùng cho: Kiểm tra quyền truy cập
    /// - Ví dụ: Không có quyền xem, không có quyền sửa
    /// </summary>
    public const string Authorization = "AUTHZ";

    /// <summary>
    /// Nhóm lỗi VALIDATE DỮ LIỆU (Validation)
    /// - Dùng cho: Kiểm tra dữ liệu đầu vào
    /// - Ví dụ: Email sai format, số điện thoại không đủ số
    /// </summary>
    public const string Validation = "VAL";

    /// <summary>
    /// Nhóm lỗi KHÔNG TÌM THẤY (Not Found)
    /// - Dùng cho: Tìm kiếm dữ liệu không có kết quả
    /// - Ví dụ: Không tìm thấy user theo ID, không tìm thấy sản phẩm
    /// </summary>
    public const string NotFound = "NF";

    /// <summary>
    /// Nhóm lỗi XUNG ĐỘT DỮ LIỆU (Conflict)
    /// - Dùng cho: Dữ liệu bị trùng lặp hoặc vi phạm ràng buộc
    /// - Ví dụ: Email đã tồn tại, SKU sản phẩm bị trùng
    /// </summary>
    public const string Conflict = "CONF";

    /// <summary>
    /// Nhóm lỗi LOGIC NGHIỆP VỤ (Business Logic)
    /// - Dùng cho: Vi phạm quy tắc nghiệp vụ
    /// - Ví dụ: Không đủ hàng trong kho, giá sale > giá gốc
    /// </summary>
    public const string Business = "BIZ";

    /// <summary>
    /// Nhóm lỗi HỆ THỐNG (System Error)
    /// - Dùng cho: Lỗi kỹ thuật, lỗi không mong muốn
    /// - Ví dụ: Database lỗi, timeout, lỗi network
    /// </summary>
    public const string System = "SYS";
}