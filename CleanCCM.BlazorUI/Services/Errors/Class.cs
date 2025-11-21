using CleanCCM.Shared.Common;

namespace CleanCCM.BlazorUI.Services.Errors;

public static class ClientErrorCatalog
{
    // Key = $"{Category}:{Code}"
    private static readonly Dictionary<string, string> _apiErrorMessages = new(StringComparer.OrdinalIgnoreCase)
    {
        // ===== BASE ERRORS =====
        ["NotFound:Base.NotFoundById"] = "Không tìm thấy dữ liệu.",
        ["Validation:Base.InvalidId"] = "Mã không hợp lệ.",
        ["Validation:Base.InvalidValue"] = "Dữ liệu không hợp lệ.",
        ["Validation:Base.OutOfRange"] = "Giá trị vượt phạm vi cho phép.",
        ["Unauthorized:Base.Unauthorized"] = "Bạn cần đăng nhập để tiếp tục.",
        ["Forbidden:Base.Forbidden"] = "Bạn không có quyền thực hiện thao tác này.",
        ["Conflict:Base.DuplicateValue"] = "Dữ liệu đã tồn tại.",

        // ===== TAG ERRORS (ví dụ) =====
        ["Conflict:Tag.DuplicateName"] = "Tên tag đã tồn tại.",
        ["Conflict:Tag.DuplicateSlug"] = "Slug của tag đã tồn tại.",
        ["Business:Tag.InUse"] = "Tag đang được sử dụng, không thể xóa.",

        // ===== CATEGORY ERRORS (ví dụ) =====
        ["Conflict:Category.DuplicateSlug"] = "Slug của category đã tồn tại."
    };

    // Lỗi thuần HTTP (không parse được ApiResult)
    private static readonly Dictionary<int, string> _httpErrorMessages = new()
    {
        [401] = "Bạn cần đăng nhập để tiếp tục.",
        [403] = "Bạn không có quyền truy cập chức năng này.",
        [404] = "Không tìm thấy tài nguyên.",
        [500] = "Hệ thống đang gặp sự cố, vui lòng thử lại sau."
    };

    public static string GetMessageForApiError(ApiError error)
    {
        var key = $"{error.Category}:{error.Code}";

        if (_apiErrorMessages.TryGetValue(key, out var msg))
            return msg;

        // fallback theo category
        return error.Category switch
        {
            "Validation" => "Dữ liệu không hợp lệ.",
            "NotFound" => "Không tìm thấy dữ liệu.",
            "Unauthorized" => "Bạn cần đăng nhập.",
            "Forbidden" => "Bạn không có quyền.",
            "Conflict" => "Xung đột dữ liệu.",
            "Business" => "Không thể thực hiện thao tác này.",
            _ => "Đã xảy ra lỗi."
        };
    }

    public static string GetMessageForHttpStatus(int statusCode, string? rawBody = null)
    {
        if (_httpErrorMessages.TryGetValue(statusCode, out var msg))
            return msg;

        if (statusCode >= 500)
            return "Hệ thống đang gặp sự cố, vui lòng thử lại sau.";

        if (statusCode >= 400)
            return "Yêu cầu không hợp lệ.";

        return "Đã xảy ra lỗi.";
    }
}
