namespace CleanCCM.Application.Common.Interfaces;

/// <summary>
/// UNIT OF WORK PATTERN - Quản lý transaction và save changes
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// UNIT OF WORK LÀ GÌ?
/// - Pattern quản lý TRANSACTION trong database
/// - Nhóm nhiều operations thành 1 transaction
/// - Đảm bảo ACID (Atomicity, Consistency, Isolation, Durability)
/// 
/// VẤN ĐỀ KHÔNG DÙNG UNIT OF WORK:
/// 
/// // CÁCH XẤU - Mỗi repository tự save
/// await _userRepository.AddAsync(user);
/// await _userRepository.SaveAsync(); // Save user
/// 
/// await _orderRepository.AddAsync(order);
/// await _orderRepository.SaveAsync(); // Save order
/// 
/// // VẤN ĐỀ: Nếu save order FAIL?
/// // → User đã được tạo (không rollback)
/// // → Data KHÔNG NHẤT QUÁN
/// 
/// CÁCH TỐT - Dùng Unit of Work:
/// 
/// await _userRepository.AddAsync(user);
/// await _orderRepository.AddAsync(order);
/// await _unitOfWork.SaveChangesAsync(); // Save TẤT CẢ cùng lúc
/// 
/// // Nếu có lỗi → ROLLBACK TẤT CẢ
/// // Data luôn NHẤT QUÁN
/// 
/// TRANSACTION LÀ GÌ?
/// - Nhóm nhiều SQL commands thành 1 đơn vị
/// - TẤT CẢ thành công hoặc TẤT CẢ thất bại
/// - KHÔNG có trạng thái giữa chừng
/// 
/// VÍ DỤ BANKING:
/// 
/// BEGIN TRANSACTION
///     UPDATE Accounts SET Balance = Balance - 100 WHERE Id = @fromAccount
///     UPDATE Accounts SET Balance = Balance + 100 WHERE Id = @toAccount
/// COMMIT
/// 
/// Nếu bất kỳ UPDATE nào fail → ROLLBACK cả 2
/// → Tiền không bị mất
/// 
/// ACID PROPERTIES:
/// 
/// A - ATOMICITY (Nguyên tử):
/// - Transaction là đơn vị nhỏ nhất
/// - Tất cả hoặc không có gì
/// 
/// C - CONSISTENCY (Nhất quán):
/// - Dữ liệu luôn ở trạng thái hợp lệ
/// - Không vi phạm constraints
/// 
/// I - ISOLATION (Cô lập):
/// - Các transaction không ảnh hưởng lẫn nhau
/// - Transaction A không thấy dữ liệu uncommitted của B
/// 
/// D - DURABILITY (Bền vững):
/// - Sau khi commit, dữ liệu được lưu vĩnh viễn
/// - Không mất dù server crash
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// LƯU TẤT CẢ THAY ĐỔI vào database
    /// 
    /// GIẢI THÍCH:
    /// - Commit tất cả operations đã thực hiện
    /// - Add, Update, Remove từ TẤT CẢ repositories
    /// - Trong 1 transaction duy nhất
    /// 
    /// VÍ DỤ SỬ DỤNG:
    /// 
    /// // Handler method
    /// public async Task<Result> Handle(CreateOrderCommand request)
    /// {
    ///     // 1. Create order
    ///     var order = Order.Create(...);
    ///     await _orderRepository.AddAsync(order);
    ///     
    ///     // 2. Update product stock
    ///     var product = await _productRepository.GetByIdAsync(productId);
    ///     product.DecreaseStock(quantity);
    ///     _productRepository.Update(product);
    ///     
    ///     // 3. Create invoice
    ///     var invoice = Invoice.Create(...);
    ///     await _invoiceRepository.AddAsync(invoice);
    ///     
    ///     // 4. SAVE TẤT CẢ trong 1 transaction
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     // Nếu bất kỳ step nào fail → ROLLBACK tất cả
    ///     return Result.Success();
    /// }
    /// 
    /// SQL ĐƯỢC THỰC THI:
    /// 
    /// BEGIN TRANSACTION
    ///     INSERT INTO Orders (...) VALUES (...)
    ///     UPDATE Products SET Stock = Stock - @quantity WHERE Id = @productId
    ///     INSERT INTO Invoices (...) VALUES (...)
    /// COMMIT
    /// 
    /// LƯU Ý:
    /// - SaveChangesAsync() tự động wrap trong transaction
    /// - Nếu có exception → tự động rollback
    /// - Nếu thành công → commit
    /// 
    /// RETURN VALUE:
    /// - int: Số records đã được modified
    /// - Ví dụ: 3 (1 order + 1 product + 1 invoice)
    /// </summary>
    /// <param name="cancellationToken">Token để cancel operation</param>
    /// <returns>Số lượng records đã được save</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// BẮT ĐẦU TRANSACTION (Manual Transaction Control)
    /// 
    /// GIẢI THÍCH:
    /// - Tạo transaction thủ công
    /// - Dùng khi cần KIỂM SOÁT transaction phức tạp
    /// - Phải gọi CommitTransactionAsync() hoặc RollbackTransactionAsync()
    /// 
    /// KHI NÀO DÙNG?
    /// - Transaction trải dài nhiều SaveChangesAsync()
    /// - Logic phức tạp cần rollback giữa chừng
    /// - Cần isolation level đặc biệt
    /// 
    /// VÍ DỤ:
    /// 
    /// try
    /// {
    ///     // Bắt đầu transaction
    ///     await _unitOfWork.BeginTransactionAsync();
    ///     
    ///     // Step 1: Create user
    ///     var user = User.Create(...);
    ///     await _userRepository.AddAsync(user);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     // Step 2: Send welcome email
    ///     await _emailService.SendWelcomeEmailAsync(user.Email);
    ///     
    ///     // Step 3: Create audit log
    ///     var auditLog = AuditLog.Create(...);
    ///     await _auditRepository.AddAsync(auditLog);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     // Commit tất cả
    ///     await _unitOfWork.CommitTransactionAsync();
    /// }
    /// catch (Exception)
    /// {
    ///     // Rollback nếu có lỗi ở bất kỳ step nào
    ///     await _unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// 
    /// FLOW:
    /// BeginTransaction
    ///   ↓
    /// SaveChanges (user) → DB chưa commit
    ///   ↓
    /// SaveChanges (audit) → DB chưa commit
    ///   ↓
    /// CommitTransaction → Commit TẤT CẢ vào DB
    /// 
    /// NẾU CÓ LỖI:
    /// BeginTransaction
    ///   ↓
    /// SaveChanges (user) → OK
    ///   ↓
    /// Email service FAIL ❌
    ///   ↓
    /// RollbackTransaction → User bị rollback
    /// 
    /// LƯU Ý:
    /// - Phải LUÔN LUÔN gọi Commit hoặc Rollback
    /// - Nếu không → connection leak, deadlock
    /// - Dùng try-finally để đảm bảo cleanup
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// COMMIT TRANSACTION - Xác nhận và lưu tất cả thay đổi
    /// 
    /// GIẢI THÍCH:
    /// - Commit transaction đã bắt đầu bởi BeginTransactionAsync()
    /// - Lưu TẤT CẢ thay đổi vào database
    /// - Sau commit → data có thể được đọc bởi transactions khác
    /// 
    /// VÍ DỤ:
    /// 
    /// await _unitOfWork.BeginTransactionAsync();
    /// try
    /// {
    ///     // Multiple operations
    ///     await _repository1.AddAsync(entity1);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     await _repository2.AddAsync(entity2);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     // Commit tất cả
    ///     await _unitOfWork.CommitTransactionAsync();
    /// }
    /// catch
    /// {
    ///     await _unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// 
    /// SQL:
    /// BEGIN TRANSACTION
    ///     INSERT INTO Table1 ...
    ///     INSERT INTO Table2 ...
    /// COMMIT TRANSACTION ← CommitTransactionAsync()
    /// 
    /// SAU KHI COMMIT:
    /// - Data được lưu vĩnh viễn
    /// - Không thể rollback nữa
    /// - Các transactions khác có thể đọc data
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// ROLLBACK TRANSACTION - Hủy bỏ tất cả thay đổi
    /// 
    /// GIẢI THÍCH:
    /// - Hủy bỏ transaction đã bắt đầu
    /// - TẤT CẢ thay đổi bị hủy
    /// - Database trở về trạng thái trước BeginTransaction
    /// 
    /// KHI NÀO DÙNG?
    /// - Có exception xảy ra
    /// - Business logic validation fail
    /// - Muốn hủy operation
    /// 
    /// VÍ DỤ:
    /// 
    /// await _unitOfWork.BeginTransactionAsync();
    /// try
    /// {
    ///     // Create order
    ///     var order = Order.Create(...);
    ///     await _orderRepository.AddAsync(order);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     // Check inventory
    ///     var product = await _productRepository.GetByIdAsync(productId);
    ///     if (product.Stock < quantity)
    ///     {
    ///         // Không đủ hàng → Rollback order
    ///         await _unitOfWork.RollbackTransactionAsync();
    ///         return Error.Business(..., "Out of stock");
    ///     }
    ///     
    ///     // Update stock
    ///     product.DecreaseStock(quantity);
    ///     _productRepository.Update(product);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     await _unitOfWork.CommitTransactionAsync();
    /// }
    /// catch (Exception)
    /// {
    ///     // Exception → Rollback
    ///     await _unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// 
    /// SQL:
    /// BEGIN TRANSACTION
    ///     INSERT INTO Orders ... ← Đã insert
    ///     -- Check stock fail
    /// ROLLBACK TRANSACTION ← RollbackTransactionAsync()
    /// -- Order bị xóa, như chưa có gì xảy ra
    /// 
    /// SAU KHI ROLLBACK:
    /// - Tất cả changes bị hủy
    /// - Database trở về trạng thái ban đầu
    /// - Có thể bắt đầu transaction mới
    /// 
    /// LƯU Ý:
    /// - LUÔN gọi Rollback trong catch block
    /// - Không gọi Rollback sau Commit (sẽ lỗi)
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}