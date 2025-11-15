namespace CleanCCM.Domain.Common;

/// <summary>
/// MARKER INTERFACE cho Aggregate Root trong Domain-Driven Design (DDD)
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// AGGREGATE ROOT LÀ GÌ?
/// - Trong DDD, entities được nhóm thành "Aggregate" (cụm)
/// - Aggregate Root là entity CHÍNH, đại diện cho cả cụm
/// - Mọi thao tác với cụm phải đi QUA root
/// 
/// VÍ DỤ THỰC TẾ:
/// 
/// 1. ORDER (Aggregate Root)
///    ├── OrderItems (entities con)
///    ├── ShippingAddress (value object)
///    └── PaymentInfo (value object)
/// 
///    → Order là root, muốn thêm/sửa/xóa OrderItem phải qua Order
///    → KHÔNG trực tiếp thao tác OrderItem
/// 
/// 2. USER (Aggregate Root)
///    ├── UserProfile (entity con)
///    ├── UserSettings (entity con)
///    └── UserAddresses (collection)
/// 
///    → User là root, thao tác profile/settings qua User
/// 
/// TẠI SAO CẦN MARKER INTERFACE?
/// - Đánh dấu class nào là Aggregate Root
/// - Repository chỉ tạo cho Aggregate Root
/// - Giúp nhận diện kiến trúc domain
/// 
/// CÁCH DÙNG:
/// public class Order : BaseAuditableEntity, IAggregateRoot
/// {
///     // Order là aggregate root
///     public List<OrderItem> Items { get; private set; }
///     
///     public void AddItem(OrderItem item)
///     {
///         // Logic thêm item phải qua Order
///         Items.Add(item);
///     }
/// }
/// 
/// public class OrderItem : BaseEntity
/// {
///     // OrderItem KHÔNG phải aggregate root
///     // KHÔNG implement IAggregateRoot
///     // KHÔNG có repository riêng
/// }
/// 
/// QUY TẮC:
/// - Chỉ tạo Repository<T> cho T implement IAggregateRoot
/// - Entities con không có repository riêng
/// - Truy cập entities con qua root
/// 
/// LƯU Ý:
/// - Đây là MARKER interface (không có method/property)
/// - Chỉ để đánh dấu, không có hành vi (behavior)
/// - Giống như ISerializable, IDisposable... trong .NET
/// </summary>
public interface IAggregateRoot
{
    // KHÔNG có members
    // Chỉ để đánh dấu (marker)
}