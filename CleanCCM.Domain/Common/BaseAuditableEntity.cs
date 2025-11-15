namespace CleanCCM.Domain.Common;

/// <summary>
/// LỚP CƠ SỞ cho các Entity CẦN AUDIT (theo dõi thay đổi)
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// - Kế thừa từ BaseEntity (có Id + DomainEvents)
/// - THÊM các trường để TRACKING: Ai tạo? Khi nào? Ai sửa? Ai xóa?
/// - Hầu hết entities trong dự án thực tế đều cần audit
/// 
/// KHI NÀO DÙNG:
/// - User, Product, Order, Category... → Kế thừa BaseAuditableEntity
/// - Cần biết ai tạo, ai sửa, khi nào sửa
/// - Soft delete (xóa mềm - đánh dấu IsDeleted thay vì xóa hẳn)
/// 
/// SOFT DELETE LÀ GÌ?
/// - Thay vì DELETE FROM Users WHERE Id = 1
/// - Ta chỉ UPDATE Users SET IsDeleted = 1 WHERE Id = 1
/// - Dữ liệu VẪN CÒN trong DB, chỉ ẩn đi
/// - Có thể khôi phục (restore) nếu cần
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    /// <summary>
    /// Thời gian TẠO entity
    /// 
    /// GIẢI THÍCH:
    /// - Tự động set = DateTime.UtcNow khi tạo entity mới
    /// - UtcNow: Giờ chuẩn quốc tế (UTC), không phụ thuộc timezone
    /// 
    /// VÍ DỤ:
    /// var user = new User(); // CreatedAt tự động = DateTime.UtcNow
    /// // CreatedAt = "2024-01-15T10:30:00Z"
    /// 
    /// TẠI SAO DÙNG UTC?
    /// - Server ở VN (UTC+7), user ở US (UTC-5)
    /// - Lưu UTC để đồng nhất, hiển thị convert sang timezone user
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// AI đã tạo entity này?
    /// 
    /// GIẢI THÍCH:
    /// - Lưu UserId hoặc Username của người tạo
    /// - Set bởi AuditableEntityInterceptor (middleware)
    /// - string? → nullable: có thể null khi tạo bởi system
    /// 
    /// VÍ DỤ:
    /// CreatedBy = "3fa85f64-5717-4562-b3fc-2c963f66afa6" (UserId)
    /// hoặc
    /// CreatedBy = "admin@example.com" (Username/Email)
    /// 
    /// NULL KHI NÀO?
    /// - Seed data (tạo bởi system, không có user)
    /// - Background job tạo
    /// - Import data
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Thời gian SỬA gần nhất
    /// 
    /// GIẢI THÍCH:
    /// - NULL khi mới tạo (chưa sửa lần nào)
    /// - Tự động update = DateTime.UtcNow mỗi khi SaveChanges
    /// - Chỉ update khi entity.State = Modified
    /// 
    /// VÍ DỤ:
    /// var user = new User(); // LastModifiedAt = null
    /// user.Name = "New Name";
    /// await _context.SaveChangesAsync(); // LastModifiedAt = DateTime.UtcNow
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// AI đã sửa lần cuối?
    /// 
    /// GIẢI THÍCH:
    /// - NULL khi chưa sửa lần nào
    /// - Update mỗi khi có modification
    /// 
    /// VÍ DỤ:
    /// LastModifiedBy = "user123" (UserId của người sửa cuối cùng)
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Đánh dấu entity đã bị XÓA chưa (soft delete)
    /// 
    /// GIẢI THÍCH:
    /// - true = đã xóa (ẩn đi, không hiện trong list)
    /// - false = chưa xóa (bình thường)
    /// - Default = false (khi tạo mới)
    /// 
    /// SOFT DELETE WORKFLOW:
    /// 1. User click "Delete"
    /// 2. Backend set IsDeleted = true (KHÔNG DELETE trong DB)
    /// 3. Query có filter: WHERE IsDeleted = false
    /// 4. User không thấy entity này nữa
    /// 5. Nhưng data VẪN CÒN trong DB
    /// 
    /// ƯU ĐIỂM:
    /// - Có thể khôi phục (restore)
    /// - Giữ lại history/audit trail
    /// - Tránh mất dữ liệu quan trọng
    /// - Tracking được ai xóa, khi nào xóa
    /// 
    /// VÍ DỤ:
    /// // Xóa soft
    /// user.IsDeleted = true;
    /// user.DeletedAt = DateTime.UtcNow;
    /// user.DeletedBy = currentUserId;
    /// await _context.SaveChangesAsync();
    /// 
    /// // Query tự động filter
    /// var users = await _context.Users.ToListAsync(); // Không có user đã xóa
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Thời gian XÓA (soft delete)
    /// 
    /// GIẢI THÍCH:
    /// - NULL khi chưa xóa
    /// - Set = DateTime.UtcNow khi xóa
    /// 
    /// VÍ DỤ:
    /// if (user.IsDeleted && user.DeletedAt.HasValue)
    ///     Console.WriteLine($"Deleted on {user.DeletedAt.Value}");
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// AI đã xóa?
    /// 
    /// GIẢI THÍCH:
    /// - NULL khi chưa xóa
    /// - UserId/Username của người thực hiện xóa
    /// 
    /// VÍ DỤ:
    /// DeletedBy = "admin_user_id" (Admin xóa entity này)
    /// 
    /// ỨNG DỤNG:
    /// - Audit: Biết ai đã xóa để trách nhiệm
    /// - Restore: Thông báo cho người đã xóa
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Constructor - Khởi tạo giá trị mặc định
    /// 
    /// GIẢI THÍCH:
    /// - Gọi base() để tạo Id (từ BaseEntity)
    /// - Set CreatedAt = UTC now
    /// - Set IsDeleted = false
    /// 
    /// VÍ DỤ:
    /// public class User : BaseAuditableEntity
    /// {
    ///     public User() : base()
    ///     {
    ///         // CreatedAt đã được set
    ///         // IsDeleted = false
    ///         // Id đã được tạo
    ///     }
    /// }
    /// </summary>
    protected BaseAuditableEntity() : base()
    {
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}