using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Common;

namespace CleanCCM.Infrastructure.Data.Interceptors;

/// <summary>
/// INTERCEPTOR TỰ ĐỘNG CẬP NHẬT AUDIT FIELDS
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// INTERCEPTOR LÀ GÌ?
/// - "Chặn" SaveChanges của EF Core
/// - Tự động cập nhật CreatedBy, LastModifiedBy, DeletedAt...
/// - Không cần manual update trong code
/// 
/// AUDIT TRAIL LÀ GÌ?
/// - Theo dõi AI tạo, AI sửa, KHI NÀO
/// - Quan trọng cho compliance, security
/// - Debug, troubleshooting
/// 
/// VÍ DỤ KHÔNG DÙNG INTERCEPTOR (XẤU):
/// 
/// var user = new User { ... };
/// user.CreatedBy = _currentUserService.UserId;  // Manual
/// user.CreatedAt = DateTime.UtcNow;             // Manual
/// await _context.SaveChangesAsync();
/// 
/// VẤN ĐỀ:
/// - Dễ quên set audit fields
/// - Code duplicate khắp nơi
/// - Không consistent
/// 
/// VỚI INTERCEPTOR (TỐT):
/// 
/// var user = new User { ... };
/// // KHÔNG cần set audit fields
/// await _context.SaveChangesAsync();
/// // Interceptor TỰ ĐỘNG set CreatedBy, CreatedAt
/// 
/// EF CORE CHANGE TRACKER:
/// - EF Core track state của entities
/// - State: Added, Modified, Deleted, Unchanged
/// - Interceptor duyệt qua tất cả tracked entities
/// 
/// SOFT DELETE:
/// - Delete → Set IsDeleted = true (không DELETE FROM)
/// - Query tự động filter WHERE IsDeleted = 0
/// - Có thể restore data
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// SERVICE LẤY THÔNG TIN USER HIỆN TẠI
    /// 
    /// GIẢI THÍCH:
    /// - Inject ICurrentUserService
    /// - Lấy UserId của user đang thực hiện action
    /// - Set vào CreatedBy, LastModifiedBy, DeletedBy
    /// </summary>
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Constructor - DI inject CurrentUserService
    /// </summary>
    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// INTERCEPT SYNCHRONOUS SaveChanges
    /// 
    /// GIẢI THÍCH:
    /// - Gọi khi SaveChanges() (không async)
    /// - Hiếm khi dùng (thường dùng SaveChangesAsync)
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// INTERCEPT ASYNCHRONOUS SaveChangesAsync
    /// 
    /// GIẢI THÍCH:
    /// - Gọi khi SaveChangesAsync() (async)
    /// - Phương thức chính được dùng
    /// 
    /// FLOW:
    /// 1. Application gọi SaveChangesAsync()
    /// 2. EF Core trigger SavingChangesAsync()
    /// 3. UpdateEntities() cập nhật audit fields
    /// 4. base.SavingChangesAsync() thực thi SQL
    /// 5. Return số records affected
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// CẬP NHẬT AUDIT FIELDS CHO TẤT CẢ ENTITIES
    /// 
    /// GIẢI THÍCH:
    /// - Duyệt qua ChangeTracker.Entries
    /// - Entries chứa tất cả entities đang tracked
    /// - Cập nhật dựa trên State (Added, Modified, Deleted)
    /// 
    /// FLOW CHI TIẾT:
    /// 
    /// 1. LẤY USER ID:
    ///    var userId = _currentUserService.UserId ?? "System";
    /// 
    /// 2. DUYỆT QUA TẤT CẢ ENTITIES:
    ///    foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
    /// 
    /// 3. CHECK STATE VÀ CẬP NHẬT:
    ///    - Added: Set CreatedBy, CreatedAt
    ///    - Modified: Set LastModifiedBy, LastModifiedAt
    ///    - Deleted: Convert to soft delete
    /// 
    /// VÍ DỤ ADDED (Tạo mới):
    /// 
    /// var user = new User { Email = "test@example.com" };
    /// _context.Users.Add(user);
    /// await _context.SaveChangesAsync();
    /// 
    /// → Interceptor detect State = Added
    /// → Set user.CreatedBy = "current-user-id"
    /// → Set user.CreatedAt = DateTime.UtcNow
    /// 
    /// VÍ DỤ MODIFIED (Cập nhật):
    /// 
    /// var user = await _context.Users.FindAsync(userId);
    /// user.Name = "New Name";
    /// await _context.SaveChangesAsync();
    /// 
    /// → Interceptor detect State = Modified
    /// → Set user.LastModifiedBy = "current-user-id"
    /// → Set user.LastModifiedAt = DateTime.UtcNow
    /// 
    /// VÍ DỤ DELETED (Soft Delete):
    /// 
    /// var user = await _context.Users.FindAsync(userId);
    /// _context.Users.Remove(user);
    /// await _context.SaveChangesAsync();
    /// 
    /// → Interceptor detect State = Deleted
    /// → CHANGE State to Modified (không delete thật)
    /// → Set user.IsDeleted = true
    /// → Set user.DeletedBy = "current-user-id"
    /// → Set user.DeletedAt = DateTime.UtcNow
    /// 
    /// SQL:
    /// -- Không phải DELETE
    /// UPDATE Users 
    /// SET IsDeleted = 1, 
    ///     DeletedBy = 'user-id', 
    ///     DeletedAt = '2024-01-15 10:30:00'
    /// WHERE Id = 'xxx'
    /// </summary>
    /// <param name="context">DbContext hiện tại</param>
    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        // LẤY USER ID HIỆN TẠI
        // Nếu không có user (background job, seed data...) → "System"
        var userId = _currentUserService.UserId ?? "System";

        // DUYỆT QUA TẤT CẢ ENTITIES kế thừa BaseAuditableEntity
        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            // ========== ADDED (Tạo mới) ==========
            if (entry.State == EntityState.Added)
            {
                // Set CreatedBy và CreatedAt
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedAt = DateTime.UtcNow;

                // VÍ DỤ:
                // User { CreatedBy = "3fa85f64...", CreatedAt = "2024-01-15 10:30:00" }
            }

            // ========== MODIFIED (Cập nhật) ==========
            // Chỉ cập nhật nếu entity THỰC SỰ thay đổi
            if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                // Set LastModifiedBy và LastModifiedAt
                entry.Entity.LastModifiedBy = userId;
                entry.Entity.LastModifiedAt = DateTime.UtcNow;

                // VÍ DỤ:
                // User { 
                //   LastModifiedBy = "3fa85f64...", 
                //   LastModifiedAt = "2024-01-15 11:00:00" 
                // }
            }

            // ========== DELETED (Soft Delete) ==========
            if (entry.State == EntityState.Deleted)
            {
                // CHUYỂN từ Delete → Update (Soft Delete)
                entry.State = EntityState.Modified;

                // Set audit fields
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedBy = userId;
                entry.Entity.DeletedAt = DateTime.UtcNow;

                // SQL:
                // UPDATE Users 
                // SET IsDeleted = 1, 
                //     DeletedBy = '3fa85f64...', 
                //     DeletedAt = '2024-01-15 12:00:00'
                // WHERE Id = 'xxx'

                // KHÔNG PHẢI:
                // DELETE FROM Users WHERE Id = 'xxx'
            }
        }
    }
}

/// <summary>
/// EXTENSION METHODS CHO EntityEntry
/// </summary>
public static class Extensions
{
    /// <summary>
    /// KIỂM TRA OWNED ENTITIES CÓ THAY ĐỔI KHÔNG
    /// 
    /// GIẢI THÍCH:
    /// 
    /// OWNED ENTITY LÀ GÌ?
    /// - Entity thuộc sở hữu của entity khác
    /// - Không có identity riêng
    /// - Ví dụ: Address owned by User
    /// 
    /// VÍ DỤ:
    /// public class User : BaseAuditableEntity
    /// {
    ///     public Address HomeAddress { get; set; }  // Owned
    /// }
    /// 
    /// public class Address  // Owned entity
    /// {
    ///     public string Street { get; set; }
    ///     public string City { get; set; }
    /// }
    /// 
    /// // Configuration
    /// builder.OwnsOne(u => u.HomeAddress);
    /// 
    /// CÁCH HOẠT ĐỘNG:
    /// 
    /// var user = await _context.Users.FindAsync(userId);
    /// user.HomeAddress.City = "Ha Noi";  // Thay đổi owned entity
    /// await _context.SaveChangesAsync();
    /// 
    /// → user.State = Unchanged (vì user không đổi)
    /// → user.HomeAddress.State = Modified (owned entity đổi)
    /// → HasChangedOwnedEntities() = true
    /// → Update LastModifiedBy, LastModifiedAt cho User
    /// 
    /// TẠI SAO CẦN?
    /// - User entity không đổi, nhưng owned entity đổi
    /// - Vẫn cần update LastModifiedAt của User
    /// - Thể hiện "User đã được modify"
    /// </summary>
    public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            r.TargetEntry.State == EntityState.Modified);
}