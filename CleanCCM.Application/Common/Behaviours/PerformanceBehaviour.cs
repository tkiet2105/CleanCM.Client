using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCCM.Application.Common.Behaviours;

/// <summary>
/// PERFORMANCE BEHAVIOUR - Theo dõi và log performance của requests
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// PERFORMANCE BEHAVIOUR LÀ GÌ?
/// - Pipeline behaviour để ĐO THỜI GIAN thực thi request
/// - Log WARNING nếu request CHẠY QUÁ LÂU
/// - Giúp phát hiện PERFORMANCE BOTTLENECKS
/// 
/// TẠI SAO CẦN THEO DÕI PERFORMANCE?
/// 
/// 1. PHÁT HIỆN SLOW QUERIES:
///    - Query database không tối ưu
///    - N+1 query problem
///    - Missing indexes
/// 
/// 2. PHÁT HIỆN SLOW OPERATIONS:
///    - External API calls chậm
///    - File I/O operations
///    - Complex calculations
/// 
/// 3. USER EXPERIENCE:
///    - Response time > 1s → Bad UX
///    - Cần optimize hoặc cache
/// 
/// VÍ DỤ THỰC TẾ:
/// 
/// // Request nhanh (50ms)
/// GetUserByIdQuery → 50ms → OK, không log
/// 
/// // Request chậm (1200ms)
/// GetAllUsersWithOrdersQuery → 1200ms 
/// → LOG WARNING: "Long Running Request: GetAllUsersWithOrdersQuery (1200ms)"
/// → Developer kiểm tra và optimize
/// 
/// CÁCH TỐI ƯU:
/// 1. Add pagination
/// 2. Add indexes
/// 3. Use caching
/// 4. Optimize query (Include, Select)
/// 
/// STOPWATCH LÀ GÌ?
/// - Class trong System.Diagnostics
/// - Đo thời gian chính xác (high-resolution timer)
/// - Tốt hơn DateTime.Now (không chính xác)
/// 
/// VÍ DỤ STOPWATCH:
/// 
/// var stopwatch = Stopwatch.StartNew();
/// 
/// // Do some work
/// await DoSomethingAsync();
/// 
/// stopwatch.Stop();
/// Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}ms");
/// 
/// FLOW HOẠT ĐỘNG:
/// 
/// Request
///   ↓
/// PerformanceBehaviour START
///   ↓ (start timer)
/// ValidationBehaviour
///   ↓
/// Handler (business logic)
///   ↓
/// PerformanceBehaviour STOP
///   ↓ (stop timer, check threshold)
/// Response
/// 
/// THRESHOLD LÀ GÌ?
/// - Ngưỡng thời gian cho phép
/// - Nếu vượt quá → Log warning
/// - Default trong code này: 500ms
/// 
/// VÍ DỤ:
/// - Request < 500ms → OK, không log
/// - Request >= 500ms → WARNING log
/// </summary>
/// <typeparam name="TRequest">Type của request (Command/Query)</typeparam>
/// <typeparam name="TResponse">Type của response</typeparam>
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// STOPWATCH - Đồng hồ bấm giờ để đo thời gian
    /// 
    /// GIẢI THÍCH:
    /// - Tạo instance mới cho mỗi request
    /// - Chính xác đến microsecond
    /// - Không bị ảnh hưởng bởi system clock changes
    /// 
    /// VÍ DỤ:
    /// _timer = new Stopwatch();
    /// _timer.Start();
    /// 
    /// // ... request processing ...
    /// 
    /// _timer.Stop();
    /// var elapsed = _timer.ElapsedMilliseconds; // 1250ms
    /// </summary>
    private readonly Stopwatch _timer;

    /// <summary>
    /// LOGGER - Để ghi log
    /// 
    /// GIẢI THÍCH:
    /// - ILogger<TRequest>: Logger cho request type cụ thể
    /// - Mỗi request type có logger riêng
    /// - Dễ filter logs theo request
    /// 
    /// VÍ DỤ LOG OUTPUT:
    /// [Warning] CleanCCM.Application.Features.Users.Queries.GetAllUsers.GetAllUsersQuery:
    ///     Long Running Request: GetAllUsersQuery (1250ms) { PageSize: 100, SearchTerm: "john" }
    /// 
    /// LOG LEVELS:
    /// - Trace: Chi tiết nhất (debug)
    /// - Debug: Thông tin debug
    /// - Information: Thông tin chung
    /// - Warning: Cảnh báo (dùng cho slow requests)
    /// - Error: Lỗi
    /// - Critical: Lỗi nghiêm trọng
    /// </summary>
    private readonly ILogger<TRequest> _logger;

    /// <summary>
    /// Constructor - Khởi tạo stopwatch và logger
    /// 
    /// GIẢI THÍCH:
    /// - DI tự động inject ILogger
    /// - Tạo Stopwatch mới cho mỗi instance
    /// 
    /// LIFETIME:
    /// - PerformanceBehaviour được tạo mới cho MỖI REQUEST
    /// - Không share state giữa các requests
    /// - Thread-safe
    /// 
    /// VÍ DỤ:
    /// // Request 1
    /// var behaviour1 = new PerformanceBehaviour<GetUserQuery, Result<User>>(logger);
    /// 
    /// // Request 2 (instance mới)
    /// var behaviour2 = new PerformanceBehaviour<CreateUserCommand, Result>(logger);
    /// </summary>
    /// <param name="logger">Logger service từ DI</param>
    public PerformanceBehaviour(ILogger<TRequest> logger)
    {
        _timer = new Stopwatch();
        _logger = logger;
    }

    /// <summary>
    /// HANDLE METHOD - Logic chính của behaviour
    /// 
    /// GIẢI THÍCH:
    /// - Wrap request execution với timer
    /// - Đo thời gian từ start đến finish
    /// - Log nếu quá lâu
    /// 
    /// FLOW CHI TIẾT:
    /// 
    /// 1. START TIMER:
    ///    _timer.Start();
    ///    Thời điểm: T0
    /// 
    /// 2. GỌI HANDLER:
    ///    var response = await next();
    ///    Handler chạy: T0 → T1
    /// 
    /// 3. STOP TIMER:
    ///    _timer.Stop();
    ///    Thời điểm: T1
    ///    Elapsed = T1 - T0
    /// 
    /// 4. KIỂM TRA THRESHOLD:
    ///    if (elapsed > 500ms)
    ///        LOG WARNING
    /// 
    /// 5. RETURN RESPONSE:
    ///    return response;
    /// 
    /// VÍ DỤ TIMELINE:
    /// 
    /// 0ms    ─┐ START TIMER
    ///         │
    /// 50ms    │ ValidationBehaviour
    ///         │
    /// 100ms   │ Handler starts
    ///         │
    /// 500ms   │ Database query
    ///         │
    /// 1200ms  │ Handler finishes
    ///         │
    /// 1250ms ─┘ STOP TIMER
    ///         ↓
    ///     LOG WARNING (elapsed = 1250ms > 500ms)
    /// 
    /// LOG OUTPUT MẪU:
    /// 
    /// [2024-01-15 10:30:45] [Warning] CleanCCM.Application.Features.Users.Queries.GetAllUsers
    /// Long Running Request: GetAllUsersQuery (1250 milliseconds)
    /// {
    ///   "PageNumber": 1,
    ///   "PageSize": 100,
    ///   "SearchTerm": "john",
    ///   "SortBy": "CreatedAt"
    /// }
    /// 
    /// PHÂN TÍCH LOG:
    /// - Request name: GetAllUsersQuery
    /// - Elapsed time: 1250ms
    /// - Request details: Serialized request object (JSON)
    /// 
    /// ACTION ITEMS:
    /// 1. Check database query
    /// 2. Add pagination (PageSize = 100 quá lớn?)
    /// 3. Add indexes
    /// 4. Consider caching
    /// 
    /// THRESHOLD CONFIGURATION:
    /// 
    /// Hiện tại: 500ms (hardcoded)
    /// 
    /// TỐT HƠN (configurable):
    /// // appsettings.json
    /// {
    ///   "Performance": {
    ///     "SlowRequestThresholdMs": 500
    ///   }
    /// }
    /// 
    /// // Code
    /// private readonly PerformanceSettings _settings;
    /// if (elapsedMilliseconds > _settings.SlowRequestThresholdMs)
    ///     _logger.LogWarning(...);
    /// 
    /// DIFFERENT THRESHOLDS:
    /// - Read operations: 500ms
    /// - Write operations: 1000ms
    /// - Complex reports: 5000ms
    /// - Batch operations: 30000ms
    /// </summary>
    /// <param name="request">Request object</param>
    /// <param name="next">Next handler trong pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response từ handler</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // BƯỚC 1: BẮT ĐẦU ĐO THỜI GIAN
        _timer.Start();

        // BƯỚC 2: GỌI HANDLER TIẾP THEO
        // Chạy tất cả behaviours và handler còn lại
        // Đây là phần TỐN THỜI GIAN nhất
        var response = await next();

        // BƯỚC 3: DỪNG ĐO THỜI GIAN
        _timer.Stop();

        // BƯỚC 4: LẤY THỜI GIAN ĐÃ TRÔI QUA (milliseconds)
        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        // BƯỚC 5: KIỂM TRA THRESHOLD (500ms)
        if (elapsedMilliseconds > 500) // TODO: Make this configurable
        {
            // REQUEST CHẠY QUÁ LÂU → LOG WARNING

            // Lấy tên request type
            var requestName = typeof(TRequest).Name;

            // Log với 3 thông tin:
            // 1. Request name
            // 2. Elapsed time
            // 3. Request details (serialized)
            _logger.LogWarning(
                "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                requestName,           // GetAllUsersQuery
                elapsedMilliseconds,   // 1250
                request                // { PageNumber: 1, PageSize: 100, ... }
            );

            // {@Request} vs {Request}:
            // {@Request} → Serialize object thành JSON (structured logging)
            // {Request} → ToString() của object

            // VÍ DỤ OUTPUT:
            // {@Request} → { "pageNumber": 1, "pageSize": 100 }
            // {Request} → CleanCCM.Application.Features.Users.Queries.GetAllUsersQuery
        }

        // BƯỚC 6: RETURN RESPONSE
        // Response đã được tạo bởi handler
        return response;
    }
}