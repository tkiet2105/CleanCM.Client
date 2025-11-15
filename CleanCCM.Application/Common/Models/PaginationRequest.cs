namespace CleanCCM.Application.Common.Models;

/// <summary>
/// MODEL CHỨA CÁC THAM SỐ CHO PHÂN TRANG VÀ TÌM KIẾM
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// PHÂN TRANG (PAGINATION) LÀ GÌ?
/// - Chia dữ liệu thành nhiều trang
/// - Mỗi trang hiển thị số lượng item nhất định
/// - Giúp tải dữ liệu nhanh, UX tốt
/// 
/// VÍ DỤ THỰC TẾ:
/// - Google search: 10 kết quả/trang
/// - Facebook feed: 20 posts/trang
/// - E-commerce: 24 sản phẩm/trang
/// 
/// TẠI SAO CẦN PAGINATION?
/// 1. PERFORMANCE:
///    - Không tải 10,000 records cùng lúc
///    - Chỉ tải 10-50 records/lần
///    - Database query nhanh hơn
///    - Network transfer ít hơn
/// 
/// 2. USER EXPERIENCE:
///    - Hiển thị nhanh
///    - Scroll mượt mà
///    - Không overwhelming
/// 
/// CÁCH HOẠT ĐỘNG:
/// 
/// CLIENT REQUEST:
/// GET /api/users?pageNumber=2&pageSize=10&sortBy=CreatedAt&sortDescending=true
/// 
/// SERVER XỬ LÝ:
/// var request = new PaginationRequest
/// {
///     PageNumber = 2,
///     PageSize = 10,
///     SortBy = "CreatedAt",
///     SortDescending = true
/// };
/// 
/// QUERY:
/// SELECT * FROM Users
/// ORDER BY CreatedAt DESC
/// OFFSET 10 ROWS      -- Skip (page 1)
/// FETCH NEXT 10 ROWS  -- Take (page 2)
/// 
/// KẾT QUẢ:
/// - Trả về 10 users của trang 2
/// - Kèm theo total count, total pages
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// SỐ TRANG hiện tại (bắt đầu từ 1)
    /// 
    /// GIẢI THÍCH:
    /// - Page 1: Records đầu tiên
    /// - Page 2: Records tiếp theo
    /// - Default = 1 (trang đầu)
    /// 
    /// VÍ DỤ:
    /// PageNumber = 1 → Lấy 10 records đầu tiên
    /// PageNumber = 2 → Skip 10, lấy 10 tiếp
    /// PageNumber = 3 → Skip 20, lấy 10 tiếp
    /// 
    /// LƯU Ý:
    /// - Một số hệ thống bắt đầu từ 0
    /// - Hệ thống này bắt đầu từ 1 (dễ hiểu hơn cho user)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// SỐ LƯỢNG ITEMS trên mỗi trang
    /// 
    /// GIẢI THÍCH:
    /// - Default = 10 items
    /// - Client có thể thay đổi (10, 20, 50, 100)
    /// 
    /// VÍ DỤ:
    /// PageSize = 10 → Hiển thị 10 items/trang
    /// PageSize = 50 → Hiển thị 50 items/trang
    /// 
    /// BEST PRACTICES:
    /// - Nên có MAX PageSize (vd: 100)
    /// - Tránh client request PageSize = 999999
    /// - Cân nhắc performance vs UX
    /// 
    /// VALIDATION NÊN THÊM:
    /// if (request.PageSize > 100)
    ///     request.PageSize = 100; // Giới hạn max
    /// 
    /// if (request.PageSize < 1)
    ///     request.PageSize = 10; // Default nếu invalid
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// TÊN FIELD để SẮP XẾP (optional)
    /// 
    /// GIẢI THÍCH:
    /// - null = không sắp xếp (default order)
    /// - "CreatedAt" = sắp xếp theo ngày tạo
    /// - "Name" = sắp xếp theo tên
    /// 
    /// VÍ DỤ:
    /// SortBy = null → SELECT * FROM Users (no ORDER BY)
    /// SortBy = "Email" → ORDER BY Email
    /// SortBy = "CreatedAt" → ORDER BY CreatedAt
    /// 
    /// LƯU Ý BẢO MẬT:
    /// - Phải VALIDATE SortBy
    /// - Chỉ cho phép sort các field hợp lệ
    /// - Tránh SQL Injection
    /// 
    /// VALIDATION NÊN THÊM:
    /// var allowedFields = new[] { "Id", "Email", "CreatedAt", "Name" };
    /// if (!string.IsNullOrEmpty(SortBy) && !allowedFields.Contains(SortBy))
    ///     throw new ValidationException("Invalid sort field");
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// CHIỀU SẮP XẾP: Tăng dần (false) hay Giảm dần (true)
    /// 
    /// GIẢI THÍCH:
    /// - false = Ascending (A→Z, 1→10, cũ→mới)
    /// - true = Descending (Z→A, 10→1, mới→cũ)
    /// - Default = false (tăng dần)
    /// 
    /// VÍ DỤ:
    /// SortBy = "Name", SortDescending = false
    /// → ORDER BY Name ASC (A, B, C...)
    /// 
    /// SortBy = "CreatedAt", SortDescending = true
    /// → ORDER BY CreatedAt DESC (mới nhất trước)
    /// 
    /// USE CASES:
    /// - Products: Giá thấp→cao (false), cao→thấp (true)
    /// - News: Cũ→mới (false), mới→cũ (true)
    /// - Users: A→Z (false), Z→A (true)
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// TỪ KHÓA TÌM KIẾM (optional)
    /// 
    /// GIẢI THÍCH:
    /// - null/empty = không tìm kiếm (lấy tất cả)
    /// - "john" = tìm kiếm records có chứa "john"
    /// 
    /// VÍ DỤ TÌM KIẾM:
    /// SearchTerm = "john"
    /// → WHERE Name LIKE '%john%' OR Email LIKE '%john%'
    /// 
    /// SearchTerm = "iphone 15"
    /// → WHERE ProductName LIKE '%iphone 15%' OR SKU LIKE '%iphone 15%'
    /// 
    /// IMPLEMENT SEARCH:
    /// if (!string.IsNullOrWhiteSpace(request.SearchTerm))
    /// {
    ///     query = query.Where(u => 
    ///         u.Name.Contains(request.SearchTerm) ||
    ///         u.Email.Contains(request.SearchTerm)
    ///     );
    /// }
    /// 
    /// LƯU Ý PERFORMANCE:
    /// - LIKE '%term%' chậm (full table scan)
    /// - Nên dùng Full-Text Search cho production
    /// - Hoặc Elasticsearch cho search phức tạp
    /// 
    /// LƯU Ý BẢO MẬT:
    /// - Validate và sanitize SearchTerm
    /// - Tránh SQL Injection
    /// - Limit độ dài SearchTerm
    /// </summary>
    public string? SearchTerm { get; set; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// SỐ RECORDS CẦN BỎ QUA (computed)
    /// 
    /// GIẢI THÍCH:
    /// - Dùng cho SQL OFFSET
    /// - Tính từ PageNumber và PageSize
    /// 
    /// CÔNG THỨC:
    /// Skip = (PageNumber - 1) * PageSize
    /// 
    /// VÍ DỤ:
    /// Page 1, Size 10 → Skip = (1-1)*10 = 0 (không skip)
    /// Page 2, Size 10 → Skip = (2-1)*10 = 10 (skip 10 đầu)
    /// Page 3, Size 10 → Skip = (3-1)*10 = 20 (skip 20 đầu)
    /// 
    /// SQL:
    /// SELECT * FROM Users
    /// ORDER BY Id
    /// OFFSET 20 ROWS      -- Skip
    /// FETCH NEXT 10 ROWS  -- Take
    /// 
    /// LINQ:
    /// var users = await _context.Users
    ///     .OrderBy(u => u.Id)
    ///     .Skip(request.Skip)  // Bỏ qua 20 đầu
    ///     .Take(request.Take)  // Lấy 10 tiếp
    ///     .ToListAsync();
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// SỐ RECORDS CẦN LẤY (computed)
    /// 
    /// GIẢI THÍCH:
    /// - Dùng cho SQL FETCH hoặc LINQ Take
    /// - Bằng với PageSize
    /// 
    /// TẠI SAO CẦN PROPERTY NÀY?
    /// - Semantic: Skip/Take rõ ràng hơn trong code
    /// - Đôi khi Take khác PageSize (edge cases)
    /// 
    /// VÍ DỤ:
    /// Take = 10 → Lấy 10 records
    /// Take = 50 → Lấy 50 records
    /// 
    /// SỬ DỤNG:
    /// var users = query
    ///     .Skip(request.Skip)
    ///     .Take(request.Take)
    ///     .ToList();
    /// </summary>
    public int Take => PageSize;
}