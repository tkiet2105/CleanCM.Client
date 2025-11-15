namespace CleanCCM.Domain.Common;

/// <summary>
/// LỚP CƠ SỞ cho TẤT CẢ các Entity trong hệ thống
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// - Mọi entity (User, Product, Order...) đều kế thừa từ class này
/// - BaseEntity cung cấp các thuộc tính/phương thức CHUNG cho mọi entity
/// - Theo nguyên tắc DRY (Don't Repeat Yourself)
/// 
/// CHỨC NĂNG:
/// 1. Quản lý ID (primary key)
/// 2. Quản lý Domain Events (pattern nâng cao)
/// 
/// DOMAIN EVENTS LÀ GÌ?
/// - Events xảy ra trong domain (vd: UserCreated, OrderPlaced)
/// - Dùng để trigger các logic khác (vd: gửi email, log, notification)
/// - Pattern trong Domain-Driven Design (DDD)
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// ID của entity - Primary Key
    /// 
    /// GIẢI THÍCH:
    /// - Dùng Guid thay vì int để tránh conflict khi merge database
    /// - Guid.NewGuid() tạo ID unique toàn cầu
    /// - protected set: Chỉ class này và class con mới set được
    /// 
    /// VÍ DỤ:
    /// var user = new User(); // Id tự động được tạo = Guid.NewGuid()
    /// Console.WriteLine(user.Id); // "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Danh sách các Domain Events của entity này
    /// 
    /// GIẢI THÍCH:
    /// - Mỗi entity có thể có nhiều events
    /// - Events được add vào list, sau đó publish/dispatch
    /// - _domainEvents là private để bảo vệ, chỉ truy cập qua methods
    /// </summary>
    private readonly List<object> _domainEvents = new();

    /// <summary>
    /// Truy cập READONLY danh sách domain events
    /// 
    /// LƯU Ý:
    /// - IReadOnlyCollection → KHÔNG thể Add/Remove từ bên ngoài
    /// - Chỉ có thể đọc (read-only)
    /// - Muốn thêm event phải dùng method AddDomainEvent()
    /// </summary>
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Constructor mặc định - Tạo ID tự động
    /// 
    /// GIẢI THÍCH:
    /// - protected: Chỉ class này và class con gọi được
    /// - Tự động tạo Id = Guid.NewGuid()
    /// 
    /// VÍ DỤ:
    /// public class User : BaseEntity
    /// {
    ///     public User() : base() { } // Gọi constructor này
    /// }
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Constructor với ID truyền vào (dùng khi load từ DB)
    /// 
    /// GIẢI THÍCH:
    /// - Khi load entity từ database, ID đã có sẵn
    /// - Constructor này set ID = giá trị từ DB
    /// 
    /// VÍ DỤ:
    /// // Entity Framework load từ DB
    /// var user = new User(existingId); // Id = existingId từ DB
    /// </summary>
    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Thêm một domain event vào entity
    /// 
    /// CÁCH DÙNG:
    /// var user = new User();
    /// user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));
    /// 
    /// SAU ĐÓ:
    /// - SaveChangesAsync sẽ dispatch tất cả events
    /// - Event handlers sẽ xử lý (gửi email, log...)
    /// </summary>
    public void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Xóa một event cụ thể khỏi danh sách
    /// 
    /// CÁCH DÙNG:
    /// user.RemoveDomainEvent(specificEvent);
    /// 
    /// KHI NÀO DÙNG:
    /// - Cancel một event trước khi save
    /// - Thay đổi logic và không muốn trigger event nữa
    /// </summary>
    public void RemoveDomainEvent(object domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Xóa TẤT CẢ domain events
    /// 
    /// CÁCH DÙNG:
    /// user.ClearDomainEvents();
    /// 
    /// KHI NÀO DÙNG:
    /// - Sau khi đã dispatch tất cả events
    /// - Đặt lại trạng thái sạch cho entity
    /// - Thường gọi trong SaveChangesAsync sau khi publish events
    /// 
    /// VÍ DỤ:
    /// await DispatchEventsAsync(); // Publish tất cả events
    /// entity.ClearDomainEvents();   // Clear sau khi đã publish
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}