using Microsoft.AspNetCore.Identity;
using CleanCCM.Infrastructure.Identity;

namespace CleanCCM.Infrastructure.Data;

/// <summary>
/// SEED DỮ LIỆU MẶC ĐỊNH CHO DATABASE
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// SEEDING LÀ GÌ?
/// - Tạo dữ liệu ban đầu cho database
/// - Roles, Admin user, default data...
/// - Chạy khi application start lần đầu
/// 
/// TẠI SAO CẦN SEEDING?
/// 
/// 1. ROLES:
///    - Application cần roles (Admin, User, Manager...)
///    - Phải tồn tại TRƯỚC KHI assign cho users
/// 
/// 2. ADMIN ACCOUNT:
///    - Cần admin để quản lý hệ thống
///    - Không thể tạo admin nếu chưa có admin
///    - Chicken-egg problem → Seed admin đầu tiên
/// 
/// 3. DEFAULT DATA:
///    - Categories, Settings, Master data...
///    - Data cần thiết để app hoạt động
/// 
/// KHI NÀO SEEDING CHẠY?
/// - Application startup (Program.cs)
/// - Sau khi migrations applied
/// - Chỉ seed nếu chưa có data (idempotent)
/// 
/// IDEMPOTENT LÀ GÌ?
/// - Chạy nhiều lần = Chạy 1 lần
/// - Không duplicate data
/// - Check exist trước khi insert
/// 
/// VÍ DỤ:
/// 
/// // Seed 1 lần
/// await SeedDefaultRolesAsync(roleManager);
/// → Roles được tạo: Admin, User, Manager
/// 
/// // Seed lần 2 (app restart)
/// await SeedDefaultRolesAsync(roleManager);
/// → Check roles đã tồn tại → Skip
/// → Không duplicate
/// </summary>
public static class ApplicationDbContextSeed
{
    /// <summary>
    /// SEED DEFAULT ROLES
    /// 
    /// GIẢI THÍCH:
    /// - Tạo roles cơ bản cho application
    /// - Admin, User, Manager
    /// - Check exist trước khi tạo (idempotent)
    /// 
    /// ROLES TRONG HỆ THỐNG:
    /// 
    /// 1. ADMIN:
    ///    - Full access
    ///    - Manage users, roles, settings
    ///    - Highest privilege
    /// 
    /// 2. USER:
    ///    - Normal user
    ///    - Limited access
    ///    - Default role khi register
    /// 
    /// 3. MANAGER:
    ///    - Middle level
    ///    - Manage content, orders...
    ///    - More access than User, less than Admin
    /// 
    /// CÁCH DÙNG TRONG CODE:
    /// 
    /// // Check role
    /// if (User.IsInRole("Admin"))
    /// {
    ///     // Admin-only logic
    /// }
    /// 
    /// // Authorize attribute
    /// [Authorize(Roles = "Admin,Manager")]
    /// public async Task<IActionResult> DeleteUser(Guid id)
    /// {
    ///     // Only Admin or Manager can access
    /// }
    /// 
    /// // Assign role
    /// await _userManager.AddToRoleAsync(user, "User");
    /// 
    /// VÍ DỤ THỰC TẾ:
    /// 
    /// // Lần đầu chạy app
    /// Roles table: EMPTY
    /// → SeedDefaultRolesAsync()
    /// → Create: Admin, User, Manager
    /// 
    /// // Lần 2 chạy app (restart)
    /// Roles table: [Admin, User, Manager]
    /// → SeedDefaultRolesAsync()
    /// → Check: Admin exists? YES → Skip
    /// → Check: User exists? YES → Skip
    /// → Check: Manager exists? YES → Skip
    /// → Không tạo gì cả (idempotent)
    /// </summary>
    /// <param name="roleManager">RoleManager từ ASP.NET Identity</param>
    public static async Task SeedDefaultRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        // DANH SÁCH ROLES CẦN SEED
        // Có thể thêm roles khác tùy business requirements
        var roles = new[] { "Admin", "Manager", "Moderator", "Support", "Editor", "Partner", "User" };

        // DUYỆT QUA TỪNG ROLE
        foreach (var role in roles)
        {
            // CHECK ROLE ĐÃ TỒN TẠI CHƯA
            if (!await roleManager.RoleExistsAsync(role))
            {
                // CHƯA TỒN TẠI → TẠO MỚI
                await roleManager.CreateAsync(new IdentityRole(role));

                // SQL:
                // INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                // VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
            }
            // ĐÃ TỒN TẠI → SKIP (idempotent)
        }
    }

    /// <summary>
    /// SEED DEFAULT ADMIN USER
    /// 
    /// GIẢI THÍCH:
    /// - Tạo admin user đầu tiên
    /// - Dùng để quản lý hệ thống
    /// - Check exist trước khi tạo
    /// 
    /// ADMIN CREDENTIALS (MẶC ĐỊNH):
    /// - Email: admin@cleanccm.com
    /// - Username: admin
    /// - Password: Admin@123
    /// 
    /// ⚠️ BẢO MẬT QUAN TRỌNG:
    /// 
    /// 1. ĐỔI PASSWORD NGAY SAU KHI DEPLOY:
    ///    - Password mặc định CỰC KỲ YẾU
    ///    - Ai cũng biết → dễ bị hack
    ///    - PHẢI đổi trong production
    /// 
    /// 2. HOẶC DÙNG ENVIRONMENT VARIABLE:
    ///    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
    ///    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    /// 
    /// 3. HOẶC DÙNG SECRETS MANAGER:
    ///    - Azure Key Vault
    ///    - AWS Secrets Manager
    ///    - HashiCorp Vault
    /// 
    /// FLOW:
    /// 
    /// 1. CHECK ADMIN TỒN TẠI:
    ///    var adminUser = await _userManager.FindByEmailAsync("admin@cleanccm.com");
    /// 
    /// 2. NẾU CHƯA TỒN TẠI:
    ///    → Create user
    ///    → Assign Admin role
    /// 
    /// 3. NẾU ĐÃ TỒN TẠI:
    ///    → Skip (idempotent)
    /// 
    /// VÍ DỤ:
    /// 
    /// // Lần đầu chạy
    /// Users table: EMPTY
    /// → SeedDefaultAdminAsync()
    /// → Create admin user
    /// → Assign Admin role
    /// 
    /// // Lần 2 chạy
    /// Users table: [admin@cleanccm.com, ...]
    /// → SeedDefaultAdminAsync()
    /// → Check: admin exists? YES
    /// → Skip
    /// </summary>
    /// <param name="userManager">UserManager từ ASP.NET Identity</param>
    public static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager)
    {
        // ADMIN EMAIL (có thể config từ appsettings.json)
        var adminEmail = "tkiet21590@gmail.com";

        // CHECK ADMIN ĐÃ TỒN TẠI CHƯA
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        // NẾU CHƯA TỒN TẠI → TẠO MỚI
        if (adminUser == null)
        {
            // TẠO ADMIN USER OBJECT
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,  // Auto-confirm email
                IsActive = true          // Active ngay
            };

            // CREATE USER VỚI PASSWORD
            // ⚠️ PASSWORD NÀY CỰC KỲ YẾU - PHẢI ĐỔI TRONG PRODUCTION
            await userManager.CreateAsync(adminUser, "Admin@123");

            // SQL:
            // INSERT INTO AspNetUsers (Id, UserName, Email, PasswordHash, ...)
            // VALUES (...)

            // PASSWORD ĐƯỢC HASH:
            // "Admin@123" → 
            // "AQAAAAEAACcQAAAAEG7..." (PBKDF2-HMAC-SHA256)

            // ASSIGN ADMIN ROLE
            await userManager.AddToRoleAsync(adminUser, "Admin");

            // SQL:
            // INSERT INTO AspNetUserRoles (UserId, RoleId)
            // SELECT @userId, Id FROM AspNetRoles WHERE Name = 'Admin'

            // LOG (optional)
            // Console.WriteLine($"Admin user created: {adminEmail}");
        }
        // ĐÃ TỒN TẠI → SKIP
    }

    /// <summary>
    /// SEED DEFAULT CATEGORIES (VÍ DỤ)
    /// 
    /// GIẢI THÍCH:
    /// - Ví dụ seed data khác
    /// - Categories, Settings, Master data...
    /// - Follow cùng pattern: Check exist → Create
    /// 
    /// VÍ DỤ:
    /// 
    /// public static async Task SeedDefaultCategoriesAsync(ApplicationDbContext context)
    /// {
    ///     // Check categories đã có chưa
    ///     if (await context.Categories.AnyAsync())
    ///         return; // Đã có → Skip
    ///     
    ///     // Tạo default categories
    ///     var categories = new[]
    ///     {
    ///         new Category { Name = "Electronics", Slug = "electronics" },
    ///         new Category { Name = "Clothing", Slug = "clothing" },
    ///         new Category { Name = "Books", Slug = "books" }
    ///     };
    ///     
    ///     context.Categories.AddRange(categories);
    ///     await context.SaveChangesAsync();
    /// }
    /// </summary>
}