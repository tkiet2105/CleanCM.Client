using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Common;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanCCM.Infrastructure.Data;

/// <summary>
/// DATABASE CONTEXT - Kết nối giữa code và database
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// DbContext LÀ GÌ?
/// - Đại diện cho một database session
/// - Quản lý entities và relationships
/// - Query và save data
/// - Unit of Work + Repository pattern
/// 
/// IdentityDbContext LÀ GÌ?
/// - Kế thừa DbContext
/// - THÊM tables cho ASP.NET Identity
/// - Users, Roles, Claims, Logins...
/// 
/// TABLES TỰ ĐỘNG TẠO BỞI IdentityDbContext:
/// - AspNetUsers
/// - AspNetRoles
/// - AspNetUserRoles
/// - AspNetUserClaims
/// - AspNetUserLogins
/// - AspNetUserTokens
/// - AspNetRoleClaims
/// 
/// IMPLEMENT IApplicationDbContext:
/// - Interface từ Application layer
/// - Cho phép Application dùng DbContext
/// - Tuân thủ Dependency Inversion (Clean Architecture)
/// 
/// VÍ DỤ SỬ DỤNG:
/// 
/// // Query
/// var users = await _context.Users
///     .Where(u => u.IsActive)
///     .ToListAsync();
/// 
/// // Add
/// _context.Users.Add(newUser);
/// await _context.SaveChangesAsync();
/// 
/// // Update
/// user.Name = "New Name";
/// await _context.SaveChangesAsync();  // EF auto-detect changes
/// 
/// // Delete
/// _context.Users.Remove(user);
/// await _context.SaveChangesAsync();
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    // ==================== DbSets ====================

    /// <summary>
    /// THÊM DbSets CHO CÁC ENTITIES CỦA BẠN Ở ĐÂY
    /// 
    /// VÍ DỤ:
    /// public DbSet<Product> Products => Set<Product>();
    /// public DbSet<Category> Categories => Set<Category>();
    /// public DbSet<Order> Orders => Set<Order>();
    /// public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    /// 
    /// QUY TẮC:
    /// - Tên DbSet: Số NHIỀU (Products, Orders, Categories)
    /// - Convention: Table name = DbSet name
    /// - Set<T>(): Method mới trong EF Core (thay vì property)
    /// 
    /// LƯU Ý:
    /// - Users, Roles... đã có sẵn từ IdentityDbContext
    /// - KHÔNG cần khai báo lại
    /// </summary>

    // TODO: Thêm DbSets của bạn ở đây
    // public DbSet<Product> Products => Set<Product>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Rating> Ratings => Set<Rating>();


    /// <summary>
    /// Constructor - Nhận DbContextOptions từ DI
    /// 
    /// GIẢI THÍCH:
    /// - DbContextOptions chứa configuration
    /// - Connection string, provider (SQL Server, PostgreSQL...)
    /// - Interceptors, logging...
    /// 
    /// REGISTRATION (Program.cs):
    /// services.AddDbContext<ApplicationDbContext>(options =>
    ///     options.UseSqlServer(connectionString));
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// MODEL CREATING - Cấu hình entities, relationships
    /// 
    /// GIẢI THÍCH:
    /// - Method được gọi khi tạo model
    /// - Cấu hình entities, indexes, relationships
    /// - Apply configurations từ IEntityTypeConfiguration
    /// 
    /// CÁCH HOẠT ĐỘNG:
    /// 
    /// 1. CALL BASE:
    ///    base.OnModelCreating(builder);
    ///    → Cấu hình Identity tables
    /// 
    /// 2. APPLY CONFIGURATIONS:
    ///    builder.ApplyConfigurationsFromAssembly(...)
    ///    → Tự động apply tất cả IEntityTypeConfiguration
    /// 
    /// 3. GLOBAL QUERY FILTER:
    ///    → WHERE IsDeleted = 0 cho tất cả queries
    /// 
    /// VÍ DỤ CONFIGURATION:
    /// 
    /// // Data/Configurations/UserConfiguration.cs
    /// public class UserConfiguration : IEntityTypeConfiguration<User>
    /// {
    ///     public void Configure(EntityTypeBuilder<User> builder)
    ///     {
    ///         builder.HasKey(u => u.Id);
    ///         builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
    ///         builder.HasIndex(u => u.Email).IsUnique();
    ///     }
    /// }
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // CALL BASE để configure Identity tables
        base.OnModelCreating(builder);

        // ========== APPLY CONFIGURATIONS ==========

        /// <summary>
        /// TỰ ĐỘNG APPLY TẤT CẢ IEntityTypeConfiguration
        /// 
        /// GIẢI THÍCH:
        /// - Scan assembly tìm classes implement IEntityTypeConfiguration<T>
        /// - Tự động gọi Configure() method
        /// 
        /// VÍ DỤ:
        /// // File: Data/Configurations/ProductConfiguration.cs
        /// public class ProductConfiguration : IEntityTypeConfiguration<Product>
        /// {
        ///     public void Configure(EntityTypeBuilder<Product> builder)
        ///     {
        ///         builder.HasKey(p => p.Id);
        ///         builder.Property(p => p.Name).IsRequired();
        ///         // ... more configurations
        ///     }
        /// }
        /// 
        /// → ApplyConfigurationsFromAssembly tự động apply
        /// → Không cần manual call trong OnModelCreating
        /// 
        /// ƯU ĐIỂM:
        /// - Tách biệt configuration ra files riêng
        /// - Dễ maintain, không bloat OnModelCreating
        /// - Convention-based, tự động detect
        /// </summary>
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ========== GLOBAL QUERY FILTER (SOFT DELETE) ==========

        /// <summary>
        /// TỰ ĐỘNG FILTER IsDeleted = false CHO MỌI QUERY
        /// 
        /// GIẢI THÍCH:
        /// - Duyệt qua tất cả entity types
        /// - Nếu kế thừa BaseAuditableEntity → Add query filter
        /// - Filter: WHERE IsDeleted = 0
        /// 
        /// VÍ DỤ:
        /// 
        /// // Code
        /// var users = await _context.Users.ToListAsync();
        /// 
        /// // SQL được generate
        /// SELECT * FROM Users WHERE IsDeleted = 0
        /// 
        /// // Lấy cả deleted records (ignore filter)
        /// var allUsers = await _context.Users
        ///     .IgnoreQueryFilters()  // Bỏ qua filter
        ///     .ToListAsync();
        /// 
        /// // SQL
        /// SELECT * FROM Users  // Không có WHERE IsDeleted
        /// 
        /// CÁCH HOẠT ĐỘNG:
        /// 
        /// 1. Duyệt qua tất cả entity types:
        ///    foreach (var entityType in builder.Model.GetEntityTypes())
        /// 
        /// 2. Check kế thừa BaseAuditableEntity:
        ///    if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
        /// 
        /// 3. Build filter expression:
        ///    Expression<Func<T, bool>> filter = e => e.IsDeleted == false
        /// 
        /// 4. Apply filter:
        ///    builder.Entity<T>().HasQueryFilter(filter)
        /// 
        /// KẾT QUẢ:
        /// - Mọi query tự động có WHERE IsDeleted = 0
        /// - Không cần manual filter trong code
        /// - Consistent behavior
        /// </summary>
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // CHECK entity có kế thừa BaseAuditableEntity không
            if (typeof(Domain.Common.BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                // BUILD FILTER EXPRESSION
                // Expression: e => e.IsDeleted == false
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(
                    parameter,
                    nameof(Domain.Common.BaseAuditableEntity.IsDeleted)
                );
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(
                        property,
                        System.Linq.Expressions.Expression.Constant(false)
                    ),
                    parameter
                );

                // APPLY FILTER
                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    /// <summary>
    /// SAVE CHANGES ASYNC - Override để add custom logic
    /// 
    /// GIẢI THÍCH:
    /// - Override method của DbContext
    /// - Có thể add logic TRƯỚC hoặc SAU save
    /// - Interceptor đã xử lý audit → Không cần logic thêm
    /// 
    /// VÍ DỤ CUSTOM LOGIC (nếu cần):
    /// 
    /// public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    /// {
    ///     // TRƯỚC save
    ///     ValidateEntities();  // Custom validation
    ///     
    ///     // SAVE
    ///     var result = await base.SaveChangesAsync(cancellationToken);
    ///     
    ///     // SAU save
    ///     await PublishDomainEventsAsync();  // Publish events
    ///     
    ///     return result;
    /// }
    /// 
    /// LƯU Ý:
    /// - Interceptor chạy TRONG base.SaveChangesAsync()
    /// - Logic ở đây chạy TRƯỚC/SAU interceptor
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Hiện tại không có logic custom
        // Chỉ gọi base implementation
        return await base.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Tự động cập nhật CreatedDate, ModifiedDate, IsDeleted
    /// dựa trên EntityState
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    // Soft delete: Thay vì xóa, set IsDeleted = true
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}