using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Domain.Errors.Product;

/// <summary>
/// CÁC LỖI ĐẶC THÙ CHO PRODUCT ENTITY
/// 
/// VÍ DỤ MINH HỌA:
/// - Product này để làm ví dụ cho các entity khác
/// - Khi tạo Category, Order, Cart... làm tương tự
/// 
/// QUY TẮC MÃ LỖI:
/// - Conflict:   9000 - 9099
/// - Business:   9100 - 9199
/// - Validation: 9200 - 9299
/// 
/// LƯU Ý:
/// - BaseErrors.NotFoundById → "Product not found by ID"
/// - BaseErrors.CreationFailed → "Create product failed"
/// - ProductErrors.SkuAlreadyExists → "SKU already exists" (đặc thù Product)
/// </summary>
public static class ProductErrors
{
    // ==================== PRODUCT CONFLICT ERRORS (9000-9099) ====================

    /// <summary>
    /// Mã SKU (Stock Keeping Unit) đã tồn tại
    /// 
    /// GIẢI THÍCH SKU:
    /// - SKU là mã định danh duy nhất của sản phẩm
    /// - Ví dụ: "IPHONE-15-PRO-256GB-BLACK"
    /// - Mỗi product có 1 SKU duy nhất trong hệ thống
    /// 
    /// SỬ DỤNG KHI:
    /// - Tạo product với SKU đã có
    /// - Update SKU bị trùng với product khác
    /// 
    /// VÍ DỤ:
    /// var existingSku = await _productRepository.FirstOrDefaultAsync(p => p.Sku == sku);
    /// if (existingSku != null)
    ///     return Error.Conflict(ProductErrors.SkuAlreadyExists, "SKU already exists");
    /// </summary>
    public static readonly ErrorCode SkuAlreadyExists =
        ErrorCode.Create(ErrorCategory.Conflict, 9001);

    /// <summary>
    /// Barcode đã được sử dụng
    /// 
    /// GIẢI THÍCH BARCODE:
    /// - Barcode là mã vạch quét (EAN-13, UPC...)
    /// - Ví dụ: "8934563140137"
    /// - Dùng để quét tại quầy thanh toán
    /// 
    /// SỬ DỤNG KHI:
    /// - Import sản phẩm với barcode trùng
    /// - Update barcode bị duplicate
    /// 
    /// VÍ DỤ:
    /// var existingBarcode = await _productRepository.FirstOrDefaultAsync(p => p.Barcode == barcode);
    /// if (existingBarcode != null)
    ///     return Error.Conflict(ProductErrors.BarcodeAlreadyExists, "Barcode exists");
    /// </summary>
    public static readonly ErrorCode BarcodeAlreadyExists =
        ErrorCode.Create(ErrorCategory.Conflict, 9002);

    // ==================== PRODUCT BUSINESS RULES (9100-9199) ====================

    /// <summary>
    /// Sản phẩm đang hết hàng (out of stock)
    /// 
    /// SỬ DỤNG KHI:
    /// - Add to cart nhưng product.Stock = 0
    /// - Place order nhưng không còn hàng
    /// 
    /// VÍ DỤ:
    /// if (product.Stock <= 0)
    ///     return Error.Business(ProductErrors.OutOfStock, "Product out of stock");
    /// 
    /// BUSINESS LOGIC:
    /// - Có thể cho phép đặt hàng trước (pre-order)
    /// - Hoặc notify khi có hàng trở lại
    /// </summary>
    public static readonly ErrorCode OutOfStock =
        ErrorCode.Create(ErrorCategory.Business, 9101);

    /// <summary>
    /// Số lượng trong kho không đủ
    /// 
    /// SỬ DỤNG KHI:
    /// - Đặt 10 sản phẩm nhưng chỉ còn 5 trong kho
    /// - product.Stock < requestedQuantity
    /// 
    /// VÍ DỤ:
    /// if (product.Stock < request.Quantity)
    ///     return Error.Business(ProductErrors.InsufficientQuantity, 
    ///         $"Only {product.Stock} items available");
    /// 
    /// LƯU Ý: Khác OutOfStock (= 0), đây là còn hàng nhưng KHÔNG ĐỦ
    /// </summary>
    public static readonly ErrorCode InsufficientQuantity =
        ErrorCode.Create(ErrorCategory.Business, 9102);

    /// <summary>
    /// Giá không hợp lệ (giá sale > giá gốc, giá âm...)
    /// 
    /// SỬ DỤNG KHI:
    /// - SalePrice > RegularPrice (vô lý)
    /// - Price < 0
    /// - Discount > 100%
    /// 
    /// VÍ DỤ:
    /// if (request.SalePrice > request.RegularPrice)
    ///     return Error.Business(ProductErrors.InvalidPricing, "Sale price > regular price");
    /// 
    /// if (request.Price < 0)
    ///     return Error.Business(ProductErrors.InvalidPricing, "Price must be positive");
    /// 
    /// BUSINESS RULE: Sale price phải <= Regular price
    /// </summary>
    public static readonly ErrorCode InvalidPricing =
        ErrorCode.Create(ErrorCategory.Business, 9103);

    /// <summary>
    /// Sản phẩm đã ngừng kinh doanh (discontinued)
    /// 
    /// SỬ DỤNG KHI:
    /// - Add to cart sản phẩm discontinued
    /// - Update stock cho sản phẩm không còn bán
    /// 
    /// VÍ DỤ:
    /// if (product.IsDiscontinued)
    ///     return Error.Business(ProductErrors.ProductDiscontinued, 
    ///         "Product is no longer available");
    /// 
    /// BUSINESS LOGIC:
    /// - Discontinued khác Delete
    /// - Giữ lại trong hệ thống để tracking order history
    /// - Nhưng KHÔNG cho phép đặt hàng mới
    /// </summary>
    public static readonly ErrorCode ProductDiscontinued =
        ErrorCode.Create(ErrorCategory.Business, 9104);

    /// <summary>
    /// Không thể xóa sản phẩm đang có trong đơn hàng
    /// 
    /// SỬ DỤNG KHI:
    /// - Delete product nhưng còn orders chứa product này
    /// - Foreign key constraint từ OrderItems
    /// 
    /// VÍ DỤ:
    /// var hasOrders = await _orderItemRepository.AnyAsync(oi => oi.ProductId == productId);
    /// if (hasOrders)
    ///     return Error.Conflict(ProductErrors.CannotDeleteProductWithOrders, 
    ///         "Cannot delete product with existing orders");
    /// 
    /// GIẢI PHÁP:
    /// - Dùng soft delete (IsDeleted = true)
    /// - Hoặc set IsDiscontinued = true
    /// - Giữ lại product để tracking order history
    /// </summary>
    public static readonly ErrorCode CannotDeleteProductWithOrders =
        ErrorCode.Create(ErrorCategory.Business, 9105);

    // ==================== PRODUCT VALIDATION (9200-9299) ====================

    /// <summary>
    /// SKU không hợp lệ (format, length...)
    /// 
    /// SỬ DỤNG KHI:
    /// - SKU không đúng format quy định
    /// - SKU quá ngắn/dài
    /// - SKU chứa ký tự không cho phép
    /// 
    /// VÍ DỤ trong FluentValidation:
    /// RuleFor(x => x.Sku)
    ///     .NotEmpty()
    ///     .Length(3, 50)
    ///     .Matches("^[A-Z0-9-]+$") // Chỉ cho phép chữ hoa, số, gạch ngang
    ///     .WithErrorCode(ProductErrors.InvalidSku.FullCode);
    /// 
    /// FORMAT QUY ĐỊNH (ví dụ):
    /// - Chỉ chữ hoa, số, gạch ngang
    /// - Độ dài 3-50 ký tự
    /// - Ví dụ: "IPHONE-15-PRO-256GB"
    /// </summary>
    public static readonly ErrorCode InvalidSku =
        ErrorCode.Create(ErrorCategory.Validation, 9201);

    /// <summary>
    /// Barcode không hợp lệ
    /// 
    /// SỬ DỤNG KHI:
    /// - Barcode không đúng chuẩn (EAN-13, UPC-A...)
    /// - Barcode không đủ số (EAN-13 cần 13 chữ số)
    /// - Checksum digit không đúng
    /// 
    /// VÍ DỤ:
    /// RuleFor(x => x.Barcode)
    ///     .Matches("^[0-9]{13}$") // EAN-13: 13 chữ số
    ///     .WithErrorCode(ProductErrors.InvalidBarcode.FullCode)
    ///     .Must(BeValidEan13Checksum)
    ///     .WithMessage("Invalid EAN-13 checksum");
    /// 
    /// GIẢI THÍCH EAN-13:
    /// - 13 chữ số, ví dụ: 8934563140137
    /// - Chữ số cuối là checksum (tính từ 12 số đầu)
    /// - Chuẩn quốc tế cho mã vạch sản phẩm
    /// </summary>
    public static readonly ErrorCode InvalidBarcode =
        ErrorCode.Create(ErrorCategory.Validation, 9202);

    /// <summary>
    /// Hình ảnh sản phẩm không hợp lệ
    /// 
    /// SỬ DỤNG KHI:
    /// - Image file > max size (vd: 10MB)
    /// - Image không phải jpg/png/webp
    /// - Image dimensions quá lớn/nhỏ
    /// - Image bị corrupt
    /// 
    /// VÍ DỤ:
    /// if (imageFile.Length > 10 * 1024 * 1024) // 10MB
    ///     return Error.Validation(ProductErrors.InvalidProductImage, "Image too large");
    /// 
    /// var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    /// if (!allowedTypes.Contains(imageFile.ContentType))
    ///     return Error.Validation(ProductErrors.InvalidProductImage, "Invalid image format");
    /// 
    /// QUY ĐỊNH (ví dụ):
    /// - Max size: 10MB
    /// - Format: JPG, PNG, WebP
    /// - Min dimensions: 500x500
    /// - Max dimensions: 4000x4000
    /// </summary>
    public static readonly ErrorCode InvalidProductImage =
        ErrorCode.Create(ErrorCategory.Validation, 9203);
}