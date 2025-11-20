
namespace CleanCCM.Shared.Common;

public sealed class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// String cho dễ xử lý ở client ("Validation", "NotFound"...)
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gửi ra luôn metadata cho frontend (vd: fields errors)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

public class ApiResult
{
    public bool IsSuccess { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResult Ok() => new()
    {
        IsSuccess = true
    };

    public static ApiResult Fail(ApiError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }

    public static ApiResult<T> Ok(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ApiResult<T> Fail(ApiError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}