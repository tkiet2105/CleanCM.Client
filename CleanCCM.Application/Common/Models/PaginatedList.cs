namespace CleanCCM.Application.Common.Models;

/// <summary>
/// DANH SÁCH CÓ PHÂN TRANG - Kết quả trả về từ pagination query
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// CLASS NÀY CHỨA GÌ?
/// 1. Items: Danh sách records của trang hiện tại
/// 2. Metadata: Thông tin về pagination (tổng số, tổng trang...)
/// 
/// VÍ DỤ RESPONSE JSON:
/// {
///     "items": [
///         { "id": 1, "name": "User 1" },
///         { "id": 2, "name": "User 2" },
///         ...
///         { "id": 10, "name": "User 10" }
///     ],
///     "pageNumber": 1,
///     "totalPages": 10,
///     "totalCount": 100,
///     "hasPreviousPage": false,
///     "hasNextPage": true
/// }
/// 
/// FRONTEND SỬ DỤNG:
/// - items: Hiển thị danh sách
/// - totalPages: Render pagination buttons
/// - hasPreviousPage: Enable/disable "Previous" button
/// - hasNextPage: Enable/disable "Next" button
/// 
/// VÍ DỤ UI:
/// [Previous] [1] [2] [3] ... [10] [Next]
///             ↑ pageNumber
///            (disabled nếu hasPreviousPage = false)
/// </summary>
/// <typeparam name="T">Type của items (User, Product, Order...)</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// DANH SÁCH ITEMS của trang hiện tại
    /// 
    /// GIẢI THÍCH:
    /// - Chứa records của trang đang xem
    /// - Ví dụ: 10 users, 20 products...
    /// 
    /// VÍ DỤ:
    /// Page 1 → Items = [User1, User2, ..., User10]
    /// Page 2 → Items = [User11, User12, ..., User20]
    /// 
    /// FRONTEND:
    /// items.forEach(item => {
    ///     renderItem(item);
    /// });
    /// </summary>
    public List<T> Items { get; }

    /// <summary>
    /// SỐ TRANG HIỆN TẠI (1-based)
    /// 
    /// VÍ DỤ:
    /// PageNumber = 1 → Đang ở trang 1
    /// PageNumber = 5 → Đang ở trang 5
    /// 
    /// FRONTEND:
    /// Highlight active page button
    /// [1] [2] [3] [4] [5] [6]
    ///                 ↑ active (PageNumber = 5)
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// TỔNG SỐ TRANG
    /// 
    /// CÔNG THỨC:
    /// TotalPages = Ceiling(TotalCount / PageSize)
    /// 
    /// VÍ DỤ:
    /// TotalCount = 95, PageSize = 10
    /// TotalPages = Ceiling(95/10) = 10 trang
    /// 
    /// Trang 1-9: 10 items/trang
    /// Trang 10: 5 items (còn lại)
    /// 
    /// FRONTEND:
    /// Render pagination buttons
    /// [1] [2] [3] ... [10]
    ///                  ↑ TotalPages
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// TỔNG SỐ RECORDS (tất cả các trang)
    /// 
    /// GIẢI THÍCH:
    /// - Tổng số records trong database (sau filter)
    /// - KHÔNG phải số items trong trang hiện tại
    /// 
    /// VÍ DỤ:
    /// TotalCount = 156
    /// PageSize = 10
    /// → Có 156 users tổng cộng
    /// → Chia làm 16 trang
    /// 
    /// FRONTEND:
    /// Hiển thị: "Showing 1-10 of 156 users"
    ///                           ↑ TotalCount
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// CÓ TRANG TRƯỚC KHÔNG?
    /// 
    /// GIẢI THÍCH:
    /// - true: Có trang trước (PageNumber > 1)
    /// - false: Đang ở trang đầu (PageNumber = 1)
    /// 
    /// VÍ DỤ:
    /// PageNumber = 1 → HasPreviousPage = false
    /// PageNumber = 2 → HasPreviousPage = true
    /// 
    /// FRONTEND:
    /// if (hasPreviousPage) {
    ///     enableButton('previous');
    /// } else {
    ///     disableButton('previous');
    /// }
    /// 
    /// UI:
    /// [← Previous] (disabled khi PageNumber = 1)
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// CÓ TRANG SAU KHÔNG?
    /// 
    /// GIẢI THÍCH:
    /// - true: Còn trang sau (PageNumber < TotalPages)
    /// - false: Đang ở trang cuối (PageNumber = TotalPages)
    /// 
    /// VÍ DỤ:
    /// PageNumber = 5, TotalPages = 10 → HasNextPage = true
    /// PageNumber = 10, TotalPages = 10 → HasNextPage = false
    /// 
    /// FRONTEND:
    /// if (hasNextPage) {
    ///     enableButton('next');
    /// } else {
    ///     disableButton('next');
    /// }
    /// 
    /// UI:
    /// [Next →] (disabled khi PageNumber = TotalPages)
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Constructor - Tạo PaginatedList
    /// 
    /// THAM SỐ:
    /// - items: Danh sách items của trang hiện tại
    /// - count: Tổng số records (TotalCount)
    /// - pageNumber: Số trang hiện tại
    /// - pageSize: Số items/trang
    /// 
    /// VÍ DỤ:
    /// var items = new List<User> { user1, user2, ... user10 };
    /// var paginatedList = new PaginatedList<User>(
    ///     items: items,
    ///     count: 156,      // Tổng 156 users
    ///     pageNumber: 1,   // Trang 1
    ///     pageSize: 10     // 10 users/trang
    /// );
    /// </summary>
    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;

        // TÍNH TỔNG SỐ TRANG
        // Ceiling: Làm tròn lên
        // Ví dụ: 95/10 = 9.5 → Ceiling = 10 trang
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    /// <summary>
    /// FACTORY METHOD - Tạo PaginatedList từ IEnumerable
    /// 
    /// GIẢI THÍCH:
    /// - Static method tiện lợi để tạo PaginatedList
    /// - Tự động Skip/Take dựa trên pageNumber/pageSize
    /// 
    /// SỬ DỤNG KHI:
    /// - Có IEnumerable (LINQ query, List...)
    /// - Muốn pagination IN-MEMORY
    /// 
    /// VÍ DỤ:
    /// var allUsers = await _context.Users.ToListAsync();
    /// var paginatedUsers = PaginatedList<User>.Create(
    ///     source: allUsers,
    ///     pageNumber: 2,
    ///     pageSize: 10
    /// );
    /// 
    /// KẾT QUẢ:
    /// - Skip 10 users đầu (page 1)
    /// - Lấy 10 users tiếp (page 2)
    /// - Items = [User11, User12, ... User20]
    /// 
    /// LƯU Ý PERFORMANCE:
    /// - Method này load TẤT CẢ data vào memory trước
    /// - Không hiệu quả cho dataset lớn
    /// - NÊN DÙNG: Pagination tại database level
    /// 
    /// CÁCH TỐT HƠN:
    /// var query = _context.Users;
    /// var count = await query.CountAsync();
    /// var items = await query
    ///     .Skip((pageNumber - 1) * pageSize)
    ///     .Take(pageSize)
    ///     .ToListAsync();
    /// return new PaginatedList<User>(items, count, pageNumber, pageSize);
    /// </summary>
    /// <param name="source">Nguồn dữ liệu (IEnumerable)</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số items/trang</param>
    /// <returns>PaginatedList với pagination đã apply</returns>
    public static PaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        // COUNT tổng số items
        var count = source.Count();

        // SKIP + TAKE để lấy items của trang hiện tại
        // Skip: Bỏ qua (pageNumber - 1) * pageSize items
        // Take: Lấy pageSize items tiếp theo
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}