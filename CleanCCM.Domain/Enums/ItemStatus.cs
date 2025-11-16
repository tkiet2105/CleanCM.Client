

namespace CleanCCM.Domain.Enums;


public static class ItemStatus
{
    public const string active = nameof(active);     // Hiển thị / hoạt động bình thường
    public const string inactive = nameof(inactive); // Không hiển thị / tạm ẩn
    public const string archived = nameof(archived); // Lưu trữ, không hiển thị
}
