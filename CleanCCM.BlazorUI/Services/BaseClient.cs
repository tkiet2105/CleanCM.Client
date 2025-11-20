using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using CleanCCM.Shared.Common;

namespace CleanCCM.BlazorUI.Services;

public abstract class BaseApiClient
{
    private readonly HttpClient _client;

    protected BaseApiClient(IHttpClientFactory httpClientFactory, string clientName = "Api")
    {
        _client = httpClientFactory.CreateClient(clientName);
    }

    // ==========================
    // GET với object query
    // ==========================
    protected Task<ApiResult<T>> GetAsync<T>(
        string url,
        object? queryObject,
        CancellationToken ct = default)
    {
        var finalUrl = AppendQuery(url, queryObject);
        return GetAsync<T>(finalUrl, ct);
    }

    // GET đơn giản không query
    protected Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Get, url, null, ct);

    // Các hàm Post/Put/Delete giữ nguyên
    protected Task<ApiResult<T>> PostAsync<T>(string url, object? body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Post, url, body, ct);

    protected Task<ApiResult<T>> PutAsync<T>(string url, object? body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Put, url, body, ct);

    protected Task<ApiResult<T>> DeleteAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Delete, url, null, ct);

    // ==========================
    // CORE SEND
    // ==========================
    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        HttpResponseMessage response;
        try
        {
            response = await _client.SendAsync(request, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ApiResult<T>.Fail(new ApiError
            {
                Code = "Client.RequestCanceled",
                Type = "Client",
                Message = "Yêu cầu đã bị huỷ."
            });
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Fail(new ApiError
            {
                Code = "Client.HttpRequestException",
                Type = "Network",
                Message = ex.Message
            });
        }

        if (response.StatusCode == HttpStatusCode.NoContent)
            return ApiResult<T>.Ok(default!);

        if (response.IsSuccessStatusCode)
        {
            var okResult = await response.Content.ReadFromJsonAsync<ApiResult<T>>(cancellationToken: ct);
            if (okResult is not null)
                return okResult;

            return ApiResult<T>.Fail(new ApiError
            {
                Code = "Client.EmptyBody",
                Type = "Client",
                Message = "Phản hồi rỗng hoặc không đúng định dạng ApiResult."
            });
        }

        var error = await ReadApiErrorFromResponse(response, ct);
        return ApiResult<T>.Fail(error);
    }

    // ==========================
    // HELPER: append query
    // ==========================
    private static string AppendQuery(string url, object? queryObject)
    {
        if (queryObject is null)
            return url;

        var pairs = new List<string>();

        var props = queryObject.GetType()
                               .GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var prop in props)
        {
            var value = prop.GetValue(queryObject);
            if (value is null)
                continue;

            var str = value.ToString();
            if (string.IsNullOrWhiteSpace(str))
                continue;

            var key = Uri.EscapeDataString(prop.Name);
            var val = Uri.EscapeDataString(str);
            pairs.Add($"{key}={val}");
        }

        if (pairs.Count == 0)
            return url;

        var queryString = string.Join("&", pairs);

        // nếu url đã có ?, nối thêm bằng &
        if (url.Contains("?", StringComparison.Ordinal))
            return $"{url}&{queryString}";

        return $"{url}?{queryString}";
    }

    // ==========================
    // HELPER: đọc ApiError
    // ==========================
    private static async Task<ApiError> ReadApiErrorFromResponse(HttpResponseMessage response, CancellationToken ct)
    {
        ApiError? apiError = null;

        try
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>(cancellationToken: ct);
            apiError = envelope?.Error;
        }
        catch
        {
        }

        if (apiError is not null)
            return apiError;

        return new ApiError
        {
            Code = $"Http.{(int)response.StatusCode}",
            Type = MapHttpStatusToType(response.StatusCode),
            Message = $"Request thất bại với mã {(int)response.StatusCode}."
        };
    }

    private static string MapHttpStatusToType(HttpStatusCode status) => status switch
    {
        HttpStatusCode.BadRequest => "Validation",
        HttpStatusCode.NotFound => "NotFound",
        HttpStatusCode.Conflict => "Conflict",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.Forbidden => "Forbidden",
        HttpStatusCode.UnprocessableEntity => "Business",
        _ => "Unexpected"
    };

    private sealed class ApiErrorEnvelope
    {
        public ApiError? Error { get; init; }
    }
}
