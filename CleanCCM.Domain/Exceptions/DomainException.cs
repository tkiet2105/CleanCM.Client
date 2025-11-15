namespace CleanCCM.Domain.Exceptions;

/// <summary>
/// EXCEPTION CHO CÁC LỖI DOMAIN (Business Logic Errors)
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// DOMAIN EXCEPTION LÀ GÌ?
/// - Exception xảy ra khi VI PHẠM BUSINESS RULES trong Domain
/// - Khác với ValidationException (lỗi dữ liệu đầu vào)
/// - Khác với SystemException (lỗi kỹ thuật)
/// 
/// KHI NÀO THROW DOMAIN EXCEPTION?
/// - Vi phạm invariants (điều kiện bất biến) của entity
/// - Logic nghiệp vụ không hợp lệ
/// - Trạng thái domain không nhất quán
/// 
/// VÍ DỤ THỰC TẾ:
/// 
/// 1. ORDER DOMAIN:
///    public class Order
///    {
///        public void Cancel()
///        {
///            if (Status == OrderStatus.Shipped)
///                throw new DomainException("Cannot cancel shipped order");
///            
///            if (Status == OrderStatus.Delivered)
///                throw new DomainException("Cannot cancel delivered order");
///            
///            Status = OrderStatus.Cancelled;
///        }
///    }
/// 
/// 2. BANK ACCOUNT DOMAIN:
///    public class BankAccount
///    {
///        public void Withdraw(decimal amount)
///        {
///            if (amount > Balance)
///                throw new DomainException("Insufficient balance");
///            
///            if (amount <= 0)
///                throw new DomainException("Withdrawal amount must be positive");
///            
///            Balance -= amount;
///        }
///    }
/// 
/// 3. PRODUCT DOMAIN:
///    public class Product
///    {
///        public void UpdateStock(int quantity)
///        {
///            var newStock = Stock + quantity;
///            
///            if (newStock < 0)
///                throw new DomainException("Stock cannot be negative");
///            
///            Stock = newStock;
///        }
///    }
/// 
/// SO SÁNH VỚI CÁC EXCEPTION KHÁC:
/// 
/// - DomainException: 
///   → Order đã ship không thể cancel (business rule)
/// 
/// - ValidationException:
///   → Email không đúng format (input validation)
/// 
/// - NotFoundException:
///   → User không tồn tại (data not found)
/// 
/// - DbUpdateException:
///   → Database connection failed (system error)
/// 
/// ƯU ĐIỂM:
/// - Tách biệt rõ ràng lỗi domain vs lỗi khác
/// - Dễ catch và xử lý riêng
/// - Thể hiện rõ business rules trong code
/// 
/// CÁCH XỬ LÝ:
/// try
/// {
///     order.Cancel();
/// }
/// catch (DomainException ex)
/// {
///     // Xử lý lỗi business logic
///     return Error.Business(BaseErrors.InvalidState, ex.Message);
/// }
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Constructor mặc định - Message rỗng
    /// 
    /// CÁCH DÙNG:
    /// throw new DomainException();
    /// 
    /// LƯU Ý: Ít khi dùng, nên dùng constructor có message
    /// </summary>
    public DomainException() : base()
    {
    }

    /// <summary>
    /// Constructor với MESSAGE (khuyến khích dùng)
    /// 
    /// CÁCH DÙNG:
    /// throw new DomainException("Cannot cancel shipped order");
    /// 
    /// VÍ DỤ:
    /// if (order.Status == OrderStatus.Shipped)
    ///     throw new DomainException("Cannot cancel order that has been shipped");
    /// 
    /// BEST PRACTICE:
    /// - Message nên rõ ràng, dễ hiểu
    /// - Nói rõ NGUYÊN NHÂN vi phạm business rule
    /// - Có thể dùng để hiển thị cho user (nếu cần)
    /// </summary>
    /// <param name="message">Thông báo lỗi mô tả business rule bị vi phạm</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor với MESSAGE và INNER EXCEPTION
    /// 
    /// CÁCH DÙNG:
    /// try
    /// {
    ///     // Some domain logic
    /// }
    /// catch (Exception ex)
    /// {
    ///     throw new DomainException("Failed to process order", ex);
    /// }
    /// 
    /// KHI NÀO DÙNG?
    /// - Khi domain logic gây ra exception khác
    /// - Muốn wrap exception gốc để giữ stack trace
    /// - Debug dễ dàng hơn (có full exception chain)
    /// 
    /// VÍ DỤ:
    /// try
    /// {
    ///     var total = CalculateTotal(); // Có thể throw ArithmeticException
    /// }
    /// catch (ArithmeticException ex)
    /// {
    ///     throw new DomainException("Invalid order calculation", ex);
    /// }
    /// 
    /// KẾT QUẢ:
    /// - Exception message: "Invalid order calculation"
    /// - InnerException: ArithmeticException (exception gốc)
    /// - Stack trace đầy đủ cả 2 exceptions
    /// </summary>
    /// <param name="message">Thông báo lỗi mô tả business rule bị vi phạm</param>
    /// <param name="innerException">Exception gốc gây ra lỗi</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}