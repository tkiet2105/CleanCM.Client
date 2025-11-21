namespace CleanCCM.Shared.Common;

public class ApiResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ApiError? Error { get; set; }

    public static ApiResult Ok(string? message = null)
        => new() { Success = true, Message = message ?? string.Empty };

    public static ApiResult Fail(ApiError error, string? message = null)
        => new() { Success = false, Error = error, Message = message ?? string.Empty };
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; set; }

    public static ApiResult<T> Ok(T data, string? message = null)
        => new()
        {
            Success = true,
            Data = data,
            Message = message ?? string.Empty
        };

    public static ApiResult<T> Fail(ApiError error, string? message = null)
        => new()
        {
            Success = false,
            Error = error,
            Message = message ?? string.Empty
        };
}

/// <summary>
/// Thông tin lỗi chuẩn backend trả về cho UI
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;       // VD: "Base.NotFoundById"
    public string Category { get; set; } = string.Empty;   // VD: "NotFound", "Validation", "Business"...
    public string? Detail { get; set; }                    // Mô tả thêm từ backend (log, debug)
}
