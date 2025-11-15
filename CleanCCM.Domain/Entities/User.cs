using CleanCCM.Domain.Common;
using CleanCCM.Domain.Exceptions;

namespace CleanCCM.Domain.Entities;

/// <summary>
/// VÍ DỤ ENTITY USER - ĐỂ MINH HỌA CÁCH SỬ DỤNG DOMAIN
/// 
/// LƯU Ý: File này là VÍ DỤ, trong thực tế User được quản lý bởi Identity
/// Nhưng đây là template tốt cho các entity khác: Product, Order, Category...
/// 
/// ĐẶC ĐIỂM ENTITY TRONG DOMAIN:
/// 1. Kế thừa BaseAuditableEntity (có audit fields)
/// 2. Implement IAggregateRoot (nếu là root)
/// 3. Properties có ENCAPSULATION (private set)
/// 4. Methods thể hiện BUSINESS LOGIC
/// 5. KHÔNG có logic infrastructure (DB, API...)
/// </summary>
public class User : BaseAuditableEntity, IAggregateRoot
{
    // ==================== PROPERTIES ====================

    /// <summary>
    /// Email của user - UNIQUE trong hệ thống
    /// 
    /// ENCAPSULATION:
    /// - public get: Ai cũng đọc được
    /// - private set: Chỉ class này set được
    /// - Muốn đổi email → dùng method ChangeEmail()
    /// 
    /// TẠI SAO KHÔNG public set?
    /// - Bảo vệ business rules
    /// - Đổi email có thể cần validate, log, send notification
    /// - Tất cả logic tập trung trong method
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Username - UNIQUE, dùng để login
    /// 
    /// ENCAPSULATION: Giống Email
    /// </summary>
    public string UserName { get; private set; } = string.Empty;

    /// <summary>
    /// Tên đầy đủ của user
    /// 
    /// BUSINESS RULE: FirstName + LastName = FullName
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// FullName được TÍNH TOÁN từ FirstName + LastName
    /// 
    /// GIẢI THÍCH:
    /// - KHÔNG lưu FullName vào DB (computed property)
    /// - Luôn đúng, không bao giờ out-of-sync
    /// - Tiết kiệm storage
    /// 
    /// VÍ DỤ:
    /// user.FirstName = "John";
    /// user.LastName = "Doe";
    /// Console.WriteLine(user.FullName); // "John Doe"
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Số điện thoại - có thể NULL
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// User có đang hoạt động không?
    /// 
    /// BUSINESS RULE:
    /// - IsActive = false → Không thể login
    /// - Admin có thể deactivate user
    /// </summary>
    public bool IsActive { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// Constructor PRIVATE - Bắt buộc dùng Factory Method
    /// 
    /// GIẢI THÍCH:
    /// - KHÔNG cho phép: new User()
    /// - BẮT BUỘC: User.Create(...)
    /// 
    /// TẠI SAO?
    /// - Factory method có validation
    /// - Factory method có business logic
    /// - Đảm bảo user luôn được tạo đúng cách
    /// 
    /// VÍ DỤ SAI:
    /// var user = new User(); // COMPILE ERROR - constructor is private
    /// 
    /// VÍ DỤ ĐÚNG:
    /// var user = User.Create("john@example.com", "john123", "John", "Doe");
    /// </summary>
    private User()
    {
        IsActive = true; // Mặc định active khi tạo mới
    }

    // ==================== FACTORY METHODS ====================

    /// <summary>
    /// Factory Method - Cách DUY NHẤT để tạo User mới
    /// 
    /// GIẢI THÍCH FACTORY PATTERN:
    /// - Static method tạo object
    /// - Có validation trước khi tạo
    /// - Có thể return NULL nếu invalid
    /// - Tập trung business rules tạo object
    /// 
    /// VÍ DỤ CÁCH DÙNG:
    /// var user = User.Create(
    ///     email: "john@example.com",
    ///     userName: "john123",
    ///     firstName: "John",
    ///     lastName: "Doe"
    /// );
    /// 
    /// if (user == null)
    /// {
    ///     // Validation failed
    /// }
    /// 
    /// ƯU ĐIỂM:
    /// - Validation tại domain (không cần Application layer validation)
    /// - Business rules rõ ràng
    /// - Dễ test
    /// </summary>
    /// <param name="email">Email của user (required, unique)</param>
    /// <param name="userName">Username để login (required, unique)</param>
    /// <param name="firstName">Tên (required)</param>
    /// <param name="lastName">Họ (required)</param>
    /// <param name="phoneNumber">Số điện thoại (optional)</param>
    /// <returns>User object hoặc null nếu validation fail</returns>
    public static User? Create(
        string email,
        string userName,
        string firstName,
        string lastName,
        string? phoneNumber = null)
    {
        // VALIDATION - Kiểm tra dữ liệu trước khi tạo

        if (string.IsNullOrWhiteSpace(email))
            return null; // Email bắt buộc

        if (string.IsNullOrWhiteSpace(userName))
            return null; // Username bắt buộc

        if (string.IsNullOrWhiteSpace(firstName))
            return null; // FirstName bắt buộc

        if (string.IsNullOrWhiteSpace(lastName))
            return null; // LastName bắt buộc

        // TẠO USER với dữ liệu hợp lệ
        var user = new User
        {
            Email = email.ToLowerInvariant(), // Lowercase email
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            IsActive = true
        };

        // DOMAIN EVENT - User được tạo
        // Event này sẽ được dispatch sau SaveChanges
        // Có thể dùng để: gửi email welcome, log, notification...
        // user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));

        return user;
    }

    // ==================== BUSINESS METHODS ====================

    /// <summary>
    /// Cập nhật thông tin cá nhân của user
    /// 
    /// BUSINESS RULES:
    /// - FirstName, LastName bắt buộc
    /// - PhoneNumber có thể null
    /// 
    /// VÍ DỤ:
    /// user.UpdateProfile("Jane", "Smith", "0901234567");
    /// await _repository.SaveChangesAsync();
    /// 
    /// TẠI SAO KHÔNG dùng property setters?
    /// - Method thể hiện rõ INTENT (ý định)
    /// - Method có validation
    /// - Method có thể add domain events
    /// - Method có thể có logic phức tạp
    /// </summary>
    /// <param name="firstName">Tên mới</param>
    /// <param name="lastName">Họ mới</param>
    /// <param name="phoneNumber">Số điện thoại mới (optional)</param>
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null)
    {
        // VALIDATION
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");

        // UPDATE
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;

        // DOMAIN EVENT (nếu cần)
        // AddDomainEvent(new UserProfileUpdatedEvent(Id));
    }

    /// <summary>
    /// Thay đổi email của user
    /// 
    /// BUSINESS RULES:
    /// - Email phải hợp lệ
    /// - Email mới khác email cũ
    /// - (Trong thực tế: cần verify email mới)
    /// 
    /// VÍ DỤ:
    /// try
    /// {
    ///     user.ChangeEmail("newemail@example.com");
    /// }
    /// catch (DomainException ex)
    /// {
    ///     // Email invalid or same as current
    /// }
    /// 
    /// LƯU Ý THỰC TẾ:
    /// - Nên gửi email xác nhận đến email mới
    /// - Chỉ đổi sau khi user verify
    /// - Log lại lịch sử đổi email
    /// </summary>
    /// <param name="newEmail">Email mới</param>
    public void ChangeEmail(string newEmail)
    {
        // VALIDATION
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new DomainException("Email cannot be empty");

        if (newEmail.Equals(Email, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("New email must be different from current email");

        // BUSINESS LOGIC
        var oldEmail = Email;
        Email = newEmail.ToLowerInvariant();

        // DOMAIN EVENT
        // AddDomainEvent(new UserEmailChangedEvent(Id, oldEmail, Email));
    }

    /// <summary>
    /// Vô hiệu hóa user (deactivate)
    /// 
    /// BUSINESS RULE:
    /// - Chỉ active user mới có thể deactivate
    /// - Deactivated user không thể login
    /// 
    /// VÍ DỤ:
    /// if (user.IsActive)
    /// {
    ///     user.Deactivate();
    ///     await _repository.SaveChangesAsync();
    /// }
    /// 
    /// SỬ DỤNG:
    /// - Admin deactivate user vi phạm
    /// - User tự deactivate account
    /// - Soft delete alternative
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("User is already inactive");

        IsActive = false;

        // DOMAIN EVENT
        // AddDomainEvent(new UserDeactivatedEvent(Id));
    }

    /// <summary>
    /// Kích hoạt lại user (activate)
    /// 
    /// BUSINESS RULE:
    /// - Chỉ inactive user mới có thể activate
    /// 
    /// VÍ DỤ:
    /// if (!user.IsActive)
    /// {
    ///     user.Activate();
    ///     await _repository.SaveChangesAsync();
    /// }
    /// 
    /// SỬ DỤNG:
    /// - Admin restore user bị ban
    /// - User đăng ký lại sau khi deactivate
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new DomainException("User is already active");

        IsActive = true;

        // DOMAIN EVENT
        // AddDomainEvent(new UserActivatedEvent(Id));
    }
}
