using Microsoft.AspNetCore.Identity;
using CleanCCM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Infrastructure.Data;

public static class ApplicationDbContextSeed
{
    
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

    public static async Task SeedDefaultCategoriesAsync(ApplicationDbContext context)
    {
        // Check categories đã có chưa
        if (await context.Categories.AnyAsync())
            return; // Đã có → Skip

        // Tạo default categories
        var categories = new[]
             {
            // nhóm nguyên liệu
            Category.Create("hải sản", "set_meal", "các loại tôm, cua, cá, mực…"),
            Category.Create("thịt và gia cầm", "restaurant", "heo, bò, gà, vịt…"),
            Category.Create("rau củ", "eco", "rau xanh, củ, nấm…"),
            Category.Create("trái cây", "nutrition", "trái cây tươi"),
            Category.Create("đồ khô", "inventory_2", "hạt, đồ khô, gia vị, mì gói…"),
            Category.Create("đồ đông lạnh", "ac_unit", "thực phẩm cấp đông"),
            Category.Create("đồ tươi sống", "egg_alt", "thực phẩm còn tươi"),
        
            // nhóm theo dạng
            Category.Create("đồ ăn chế biến", "skillet", "luộc, hấp, nướng, chiên, xào…"),
            Category.Create("đồ tráng miệng", "icecream", "sữa chua, kem, chè, trái cây dầm…"),
            Category.Create("đồ uống", "local_cafe", "trà, cà phê, nước ép…"),
            Category.Create("bánh và đồ ngọt", "cake", "bánh mì, bánh kem, bánh snack…"),
        
            Category.Create("khác", "more_horiz", "không thuộc nhóm trên"),
        };





        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    public static async Task SeedDefaultTagsAsync(ApplicationDbContext context)
    {
        // Check categories đã có chưa
        if (await context.Tags.AnyAsync())
            return; // Đã có → Skip
        var tags = new[]
         {
              Tag.Create("đặc sản", "restaurant_menu", "#e67e22"),
              Tag.Create("ship tận nơi", "delivery_dining", "#34495e"),
              Tag.Create("cần đặt trước", "schedule", "#8e44ad"),
              Tag.Create("mua tại chỗ", "storefront", "#7f8c8d"),
              Tag.Create("hàng sẵn có", "check_circle", "#16a085"),
        };


        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();
    }
    //Add-Migration UpdateIcon -Project CleanCCM.Infrastructure -StartupProject CleanCCM.Api
    //Update-Database -Project CleanCCM.Infrastructure -StartupProject CleanCCM.Api

   

}