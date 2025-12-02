using Microsoft.AspNetCore.Identity;
using CleanCCM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Infrastructure.Data;

public static class ApplicationDbContextSeed
{
    //Add-Migration AddImageAndAddress -Project CleanCCM.Infrastructure -StartupProject CleanCCM.Api
    //Update-Database -Project CleanCCM.Infrastructure -StartupProject CleanCCM.Api
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



    public static async Task SeedDefaultProductsAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var categories = await context.Categories.ToListAsync();
        var tags = await context.Tags.ToListAsync();

        var catByName = categories
            .GroupBy(c => c.Name.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        var tagByName = tags
            .GroupBy(t => t.Name.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        Category Cat(string name) => catByName[name.Trim().ToLowerInvariant()];
        Tag Tg(string name) => tagByName[name.Trim().ToLowerInvariant()];

        var products = new List<Product>();
        var wards = GetWardsList();

        // ============ HẢI SẢN (4 sản phẩm) ============
        products.Add(CreateProduct(
            "Tôm sú tươi sống",
            "Tôm sú tươi đánh bắt trong ngày, size 5-7 con/kg, thịt chắc ngọt tự nhiên",
            350000, 50,
            new[] { Cat("hải sản"), Cat("đồ tươi sống") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[0],
            "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47"
        ));

        products.Add(CreateProduct(
            "Cua biển tươi nguyên con",
            "Cua biển Cà Mau, cua gạch đầy, thịt chắc ngọt, từ 500-800g/con",
            280000, 35,
            new[] { Cat("hải sản"), Cat("đồ tươi sống") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[1],
            "https://images.unsplash.com/photo-1615141982883-c7ad0e69fd62"
        ));

        products.Add(CreateProduct(
            "Mực ống tươi",
            "Mực ống tươi làm sạch, thịt trắng ngọt, phù hợp nướng, chiên, xào",
            180000, 60,
            new[] { Cat("hải sản"), Cat("đồ tươi sống") },
            new[] { Tg("đặc sản"), Tg("ship tận nơi") },
            wards[2],
            "https://images.unsplash.com/photo-1599084993091-1cb5c0721cc6"
        ));

        products.Add(CreateProduct(
            "Cá lóc đồng sống",
            "Cá lóc đồng nuôi tự nhiên, thịt chắc ít xương, từ 1-1.5kg/con",
            120000, 40,
            new[] { Cat("hải sản"), Cat("đồ tươi sống") },
            new[] { Tg("đặc sản"), Tg("hàng sẵn có") },
            wards[3],
            "https://images.unsplash.com/photo-1534043464124-3be32fe000c9"
        ));

        // ============ THỊT VÀ GIA CẦM (4 sản phẩm) ============
        products.Add(CreateProduct(
            "Ba chỉ heo tươi",
            "Ba chỉ heo thịt tươi ngày, vân mỡ đều, thịt mềm ngọt",
            95000, 80,
            new[] { Cat("thịt và gia cầm"), Cat("đồ tươi sống") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[4],
            "https://images.unsplash.com/photo-1602470520998-f4a52199a3d6"
        ));

        products.Add(CreateProduct(
            "Gà ta nguyên con",
            "Gà ta thả vườn, từ 1.2-1.5kg/con, thịt chắc thơm",
            180000, 25,
            new[] { Cat("thịt và gia cầm"), Cat("đồ tươi sống") },
            new[] { Tg("cần đặt trước"), Tg("đặc sản") },
            wards[5],
            "https://images.unsplash.com/photo-1587593810167-a84920ea0781"
        ));

        products.Add(CreateProduct(
            "Thịt bò Úc nhập khẩu",
            "Thịt bò Úc loại thăn, cắt lát mỏng, phù hợp nướng, lẩu",
            320000, 30,
            new[] { Cat("thịt và gia cầm"), Cat("đồ đông lạnh") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[6],
            "https://images.unsplash.com/photo-1588347818036-5c7e8f4ee01c"
        ));

        products.Add(CreateProduct(
            "Vịt quay bắc kinh",
            "Vịt quay nguyên con theo công thức Bắc Kinh, da giòn thịt mềm",
            450000, 15,
            new[] { Cat("thịt và gia cầm"), Cat("đồ ăn chế biến") },
            new[] { Tg("cần đặt trước"), Tg("đặc sản") },
            wards[7],
            "https://images.unsplash.com/photo-1583638421089-c19c58e3c61c"
        ));

        // ============ RAU CỦ (4 sản phẩm) ============
        products.Add(CreateProduct(
            "Rau muống tươi",
            "Rau muống tươi hữu cơ, lá xanh non, ngọt tự nhiên",
            15000, 100,
            new[] { Cat("rau củ"), Cat("đồ tươi sống") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[0],
            "https://images.unsplash.com/photo-1576045057995-568f588f82fb"
        ));

        products.Add(CreateProduct(
            "Cà chua bi",
            "Cà chua bi Đà Lạt, quả đỏ đều, ngọt thanh, giàu vitamin",
            35000, 80,
            new[] { Cat("rau củ"), Cat("đồ tươi sống") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[1],
            "https://images.unsplash.com/photo-1592924357228-91a4daadcfea"
        ));

        products.Add(CreateProduct(
            "Nấm đùi gà tươi",
            "Nấm đùi gà nuôi cấy, tươi ngon, thịt nấm dày chắc",
            60000, 45,
            new[] { Cat("rau củ"), Cat("đồ tươi sống") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[2],
            "https://images.unsplash.com/photo-1565450464207-4c6b1b3b2770"
        ));

        products.Add(CreateProduct(
            "Khoai lang tím",
            "Khoai lang tím Đà Lạt, củ to đều, ngọt bùi, giàu anthocyanin",
            25000, 90,
            new[] { Cat("rau củ"), Cat("đồ tươi sống") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[3],
            "https://images.unsplash.com/photo-1589927986089-35812378d41d"
        ));

        // ============ TRÁI CÂY (4 sản phẩm) ============
        products.Add(CreateProduct(
            "Sầu riêng Ri6",
            "Sầu riêng Ri6 Đắk Lắk, múi dày, cơm vàng, ngọt béo",
            180000, 20,
            new[] { Cat("trái cây") },
            new[] { Tg("đặc sản"), Tg("cần đặt trước") },
            wards[4],
            "https://images.unsplash.com/photo-1580910365203-91ea527b2a42"
        ));

        products.Add(CreateProduct(
            "Xoài Úc",
            "Xoài Úc loại 1, quả to, thịt vàng, ngọt thanh, ít xơ",
            65000, 50,
            new[] { Cat("trái cây") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[5],
            "https://images.unsplash.com/photo-1591073113125-e46713c829ed"
        ));

        products.Add(CreateProduct(
            "Bơ sáp Đắk Lắk",
            "Bơ sáp Đắk Lắk, quả từ 500-700g, cơm dày, béo ngậy",
            45000, 60,
            new[] { Cat("trái cây") },
            new[] { Tg("đặc sản"), Tg("ship tận nơi") },
            wards[6],
            "https://images.unsplash.com/photo-1523049673857-eb18f1d7b578"
        ));

        products.Add(CreateProduct(
            "Thanh long ruột đỏ",
            "Thanh long ruột đỏ Bình Thuận, quả to, ngọt thanh, vitamin C cao",
            30000, 75,
            new[] { Cat("trái cây") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[7],
            "https://images.unsplash.com/photo-1526318472351-c75fcf070305"
        ));

        // ============ ĐỒ KHÔ (3 sản phẩm) ============
        products.Add(CreateProduct(
            "Khô cá lóc rút xương",
            "Khô cá lóc rút xương, thịt chắc ngọt, không hóa chất",
            250000, 30,
            new[] { Cat("đồ khô") },
            new[] { Tg("đặc sản"), Tg("ship tận nơi") },
            wards[0],
            "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2"
        ));

        products.Add(CreateProduct(
            "Mắm tôm chua Cà Mau",
            "Mắm tôm chua đặc sản Cà Mau, hủ 500g, vị chua đậm đà",
            85000, 40,
            new[] { Cat("đồ khô") },
            new[] { Tg("đặc sản"), Tg("hàng sẵn có") },
            wards[1],
            "https://images.unsplash.com/photo-1563379926898-05f4575a45d8"
        ));

        products.Add(CreateProduct(
            "Hạt điều rang muối",
            "Hạt điều rang muối Bình Phước, hạt to đều, giòn ngon",
            180000, 55,
            new[] { Cat("đồ khô") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[2],
            "https://images.unsplash.com/photo-1599599810769-bcde5a160d32"
        ));

        // ============ ĐỒ ĐÔNG LẠNH (3 sản phẩm) ============
        products.Add(CreateProduct(
            "Cá hồi Na Uy phi lê",
            "Cá hồi Na Uy phi lê đông lạnh, thịt hồng tươi, giàu Omega-3",
            380000, 25,
            new[] { Cat("đồ đông lạnh"), Cat("hải sản") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[3],
            "https://images.unsplash.com/photo-1574781330855-d0db3da7de9f"
        ));

        products.Add(CreateProduct(
            "Tôm hùm đông lạnh",
            "Tôm hùm đông lạnh cắt đôi, size 300-400g/con, thịt chắc ngọt",
            550000, 15,
            new[] { Cat("đồ đông lạnh"), Cat("hải sản") },
            new[] { Tg("cần đặt trước"), Tg("đặc sản") },
            wards[4],
            "https://images.unsplash.com/photo-1559737558-2f5a2b8c6c6e"
        ));

        products.Add(CreateProduct(
            "Dim sum hải sản cao cấp",
            "Dim sum hải sản đông lạnh, nhân tôm cua, hấp ăn liền",
            120000, 40,
            new[] { Cat("đồ đông lạnh"), Cat("đồ ăn chế biến") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[5],
            "https://images.unsplash.com/photo-1563245372-f21724e3856d"
        ));

        // ============ ĐỒ ĂN CHẾ BIẾN (5 sản phẩm) ============
        products.Add(CreateProduct(
            "Bún mắm Cà Mau đặc biệt",
            "Tô bún mắm đậm vị miền Tây với tôm đất, mực, cá lóc, rau sống đầy đủ",
            45000, 100,
            new[] { Cat("đồ ăn chế biến") },
            new[] { Tg("đặc sản"), Tg("ship tận nơi") },
            wards[6],
            "https://images.unsplash.com/photo-1569562211093-4ed0d0758f12"
        ));

        products.Add(CreateProduct(
            "Lẩu mắm U Minh",
            "Lẩu mắm cá linh, cá sặc, rau rừng U Minh, phục vụ 3-4 người",
            380000, 30,
            new[] { Cat("đồ ăn chế biến"), Cat("hải sản") },
            new[] { Tg("đặc sản"), Tg("cần đặt trước") },
            wards[7],
            "https://images.unsplash.com/photo-1585032226651-759b368d7246"
        ));

        products.Add(CreateProduct(
            "Cá lóc nướng trui lá sen",
            "Cá lóc đồng nướng trui nguyên con, cuốn bánh tráng, rau sống",
            220000, 40,
            new[] { Cat("đồ ăn chế biến"), Cat("hải sản") },
            new[] { Tg("đặc sản"), Tg("mua tại chỗ") },
            wards[0],
            "https://images.unsplash.com/photo-1544025162-d76694265947"
        ));

        products.Add(CreateProduct(
            "Gỏi cuốn tôm thịt",
            "Gỏi cuốn tôm thịt tươi, rau thơm đầy đủ, 10 cuốn/phần",
            60000, 70,
            new[] { Cat("đồ ăn chế biến") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[1],
            "https://images.unsplash.com/photo-1559314809-0d155014e29e"
        ));

        products.Add(CreateProduct(
            "Cơm gà Hải Nam",
            "Cơm gà Hải Nam nguyên phần, gà luộc mềm, cơm thơm dẻo",
            55000, 80,
            new[] { Cat("đồ ăn chế biến"), Cat("thịt và gia cầm") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[2],
            "https://images.unsplash.com/photo-1512058564366-18510be2db19"
        ));

        // ============ ĐỒ TRÁNG MIỆNG (4 sản phẩm) ============
        products.Add(CreateProduct(
            "Chè ba màu",
            "Chè ba màu truyền thống, đậu xanh, đậu đỏ, thạch rau câu, nước cốt dừa",
            25000, 90,
            new[] { Cat("đồ tráng miệng") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[3],
            "https://images.unsplash.com/photo-1563805042-7684c019e1cb"
        ));

        products.Add(CreateProduct(
            "Kem dừa tươi",
            "Kem dừa tươi Bến Tre, vị béo ngậy tự nhiên, không chất bảo quản",
            35000, 60,
            new[] { Cat("đồ tráng miệng") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[4],
            "https://images.unsplash.com/photo-1563805042-7684c019e1cb"
        ));

        products.Add(CreateProduct(
            "Sữa chua nếp cẩm",
            "Sữa chua nếp cẩm Đà Lạt, men vi sinh sống, topping hoa quả tươi",
            30000, 75,
            new[] { Cat("đồ tráng miệng") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[5],
            "https://images.unsplash.com/photo-1488477181946-6428a0291777"
        ));

        products.Add(CreateProduct(
            "Bánh flan caramen",
            "Bánh flan caramen mềm mịn, béo ngậy, hộp 4 chiếc",
            40000, 50,
            new[] { Cat("đồ tráng miệng") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[6],
            "https://images.unsplash.com/photo-1578985545062-69928b1d9587"
        ));

        // ============ ĐỒ UỐNG (5 sản phẩm) ============
        products.Add(CreateProduct(
            "Cà phê sữa đá Cà Mau",
            "Cà phê phin truyền thống, hạt rang xay tại chỗ, sữa đặc",
            18000, 200,
            new[] { Cat("đồ uống") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[7],
            "https://images.unsplash.com/photo-1461023058943-07fcbe16d735"
        ));

        products.Add(CreateProduct(
            "Sinh tố bơ sáp",
            "Sinh tố bơ sáp Cà Mau, xay cùng sữa đặc, topping dừa khô",
            25000, 120,
            new[] { Cat("đồ uống") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[0],
            "https://images.unsplash.com/photo-1546173159-315724a31696"
        ));

        products.Add(CreateProduct(
            "Trà sữa trân châu đường đen",
            "Trà sữa trân châu đường đen, size L, ít đá ít đường",
            35000, 150,
            new[] { Cat("đồ uống") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[1],
            "https://images.unsplash.com/photo-1525385133512-2f3bdd039054"
        ));

        products.Add(CreateProduct(
            "Nước dừa tươi nguyên trái",
            "Nước dừa xiêm tươi nguyên trái, ngọt mát tự nhiên",
            20000, 100,
            new[] { Cat("đồ uống") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[2],
            "https://images.unsplash.com/photo-1556679343-c7306c1976bc"
        ));

        products.Add(CreateProduct(
            "Trà đào cam sả",
            "Trà đào cam sả tươi mát, đào ngâm tự nhiên, cam Vinh",
            32000, 110,
            new[] { Cat("đồ uống") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[3],
            "https://images.unsplash.com/photo-1556679086-cf0f8f21c9e0"
        ));

        // ============ BÁNH VÀ ĐỒ NGỌT (4 sản phẩm) ============
        products.Add(CreateProduct(
            "Bánh tằm bì Cà Mau",
            "Bánh tằm bì mềm, chan nước cốt dừa béo, nước mắm chua ngọt",
            30000, 80,
            new[] { Cat("bánh và đồ ngọt") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[4],
            "https://images.unsplash.com/photo-1603046891726-36bfd957f598"
        ));

        products.Add(CreateProduct(
            "Bánh mì thịt nướng đặc biệt",
            "Bánh mì thịt nướng Sài Gòn, thịt nướng thơm, đầy đủ topping",
            25000, 95,
            new[] { Cat("bánh và đồ ngọt") },
            new[] { Tg("hàng sẵn có"), Tg("ship tận nơi") },
            wards[5],
            "https://images.unsplash.com/photo-1509440159596-0249088772ff"
        ));

        products.Add(CreateProduct(
            "Bánh bông lan trứng muối",
            "Bánh bông lan trứng muối Đà Lạt, hộp 6 chiếc",
            120000, 35,
            new[] { Cat("bánh và đồ ngọt") },
            new[] { Tg("đặc sản"), Tg("cần đặt trước") },
            wards[6],
            "https://images.unsplash.com/photo-1586985289688-ca3cf47d3e6e"
        ));

        products.Add(CreateProduct(
            "Bánh Pía chay đậu xanh",
            "Bánh Pía chay Sóc Trăng, nhân đậu xanh nguyên chất, hộp 10 cái",
            85000, 45,
            new[] { Cat("bánh và đồ ngọt") },
            new[] { Tg("đặc sản"), Tg("ship tận nơi") },
            wards[7],
            "https://images.unsplash.com/photo-1603532648955-039310d9ed75"
        ));

        // ============ KHÁC (3 sản phẩm) ============
        products.Add(CreateProduct(
            "Nước mắm Phú Quốc",
            "Nước mắm Phú Quốc 35 độ đạm, chai 500ml, hương vị truyền thống",
            65000, 70,
            new[] { Cat("khác"), Cat("đồ khô") },
            new[] { Tg("đặc sản"), Tg("ship tận nơi") },
            wards[0],
            "https://images.unsplash.com/photo-1563379926898-05f4575a45d8"
        ));

        products.Add(CreateProduct(
            "Muối ớt Tây Ninh",
            "Muối ớt Tây Ninh xay nhuyễn, cay thơm, hủ 250g",
            35000, 85,
            new[] { Cat("khác"), Cat("đồ khô") },
            new[] { Tg("hàng sẵn có"), Tg("mua tại chỗ") },
            wards[1],
            "https://images.unsplash.com/photo-1599909533850-f8b6bcc6a9b1"
        ));

        products.Add(CreateProduct(
            "Thảo mộc sức khỏe",
            "Bộ thảo mộc sức khỏe gồm: trà atiso, trà hoa cúc, mật ong rừng",
            180000, 25,
            new[] { Cat("khác") },
            new[] { Tg("đặc sản"), Tg("cần đặt trước") },
            wards[2],
            "https://images.unsplash.com/photo-1563805042-7684c019e1cb"
        ));

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    private static Product CreateProduct(
        string name,
        string description,
        decimal price,
        int stock,
        Category[] categories,
        Tag[] tags,
        string ward,
        string imageUrl)
    {
        var product = Product.Create(name, description, price, stock);
        product.Publish();

        foreach (var category in categories)
        {
            product.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id));
        }

        foreach (var tag in tags)
        {
            product.ProductTags.Add(ProductTag.Create(product.Id, tag.Id));
        }

        product.Addresses.Add(Address.Create(
            line1: $"{Random.Shared.Next(1, 200)} đường Nguyễn Trãi",
            city: "Thành phố Cà Mau",
            district: "Thành phố Cà Mau",
            ward: ward,
            productId: product.Id,
            line2: "Khu ẩm thực địa phương",
            isPrimary: true
        ));

        product.Images.Add(Image.Create(
            productId: product.Id,
            fileName: $"{product.Slug}.jpg",
            url: imageUrl,
            alt: name,
            sortOrder: 0,
            isPrimary: true
        ));

        return product;
    }

    private static string[] GetWardsList()
    {
        return new[]
        {
            "Phường 1", "Phường 2", "Phường 3", "Phường 4",
            "Phường 5", "Phường 6", "Phường 7", "Phường 8","Khác"
           
        };
    }


}