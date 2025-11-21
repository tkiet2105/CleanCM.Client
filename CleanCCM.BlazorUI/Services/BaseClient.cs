using CleanCCM.BlazorUI.Extensions;
using CleanCCM.BlazorUI.Services.Errors;
using CleanCCM.Shared.Common;          // ApiResult, ApiResult<T>, ApiError
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace CleanCCM.BlazorClient.ApiClients;

/// <summary>
/// Base API client dùng cho tất cả client (ProductClient, CategoryClient,...)
/// - Tập trung xử lý gửi request
/// - Tập trung xử lý lỗi + mapping ErrorCode → message thân thiện
/// </summary>
public abstract class BaseApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected BaseApiClient(IHttpClientFactory httpClientFactory, string clientName)
    {
        _httpClient = httpClientFactory.CreateClient(clientName);
    }

    // ================== PUBLIC HELPERS ==================

    protected Task<ApiResult<T>> GetAsync<T>(
        string url,
        object? query = null,
        CancellationToken cancellationToken = default)
        => SendAsync<T>(HttpMethod.Get, url, query, null, cancellationToken);

    protected Task<ApiResult<T>> PostAsync<T>(
        string url,
        object? body = null,
        CancellationToken cancellationToken = default)
        => SendAsync<T>(HttpMethod.Post, url, null, body, cancellationToken);

    // ================== CORE SEND ==================

    private async Task<ApiResult<T>> SendAsync<T>(
        HttpMethod method,
        string url,
        object? query,
        object? body,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestUrl = BuildUrl(url, query);

            using var request = new HttpRequestMessage(method, requestUrl);

            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            // HTTP status lỗi (401, 403, 404, 500...)
            if (!response.IsSuccessStatusCode)
            {
                return await HandleErrorAsync<T>(response);
            }

            // Trường hợp success (HTTP 2xx): backend vẫn trả ApiResult<T>
            var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<T>>(
                _jsonOptions,
                cancellationToken);

            if (apiResult is null)
            {
                return ApiResult<T>.Fail(
                    new ApiError
                    {
                        Code = "Client.EmptyResponse",
                        Category = "Client",
                        Detail = "Response body is null"
                    },
                    "Không nhận được dữ liệu từ máy chủ."
                );
            }

            // Nếu backend trả Success = false nhưng HTTP vẫn 200 (lỗi business, validation...)
            if (!apiResult.Success)
            {
                if (apiResult.Error != null)
                {
                    var friendly = ClientErrorCatalog.GetMessageForApiError(apiResult.Error);

                    if (string.IsNullOrWhiteSpace(apiResult.Message))
                        apiResult.Message = friendly;
                }
                else
                {
                    // Không có Error → dùng message hoặc fallback
                    if (string.IsNullOrWhiteSpace(apiResult.Message))
                        apiResult.Message = "Yêu cầu không thành công.";
                }
            }

            return apiResult;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout
            return ApiResult<T>.Fail(
                new ApiError
                {
                    Code = "Client.Timeout",
                    Category = "Client",
                    Detail = "Request timeout"
                },
                "Yêu cầu mất quá nhiều thời gian, vui lòng thử lại."
            );
        }
        catch (HttpRequestException ex)
        {
            // Lỗi mạng
            return ApiResult<T>.Fail(
                new ApiError
                {
                    Code = "Client.NetworkError",
                    Category = "Client",
                    Detail = ex.Message
                },
                "Lỗi kết nối mạng, vui lòng kiểm tra lại đường truyền."
            );
        }
        catch (Exception ex)
        {
            // Lỗi không xác định phía client
            return ApiResult<T>.Fail(
                new ApiError
                {
                    Code = "Client.Unhandled",
                    Category = "Client",
                    Detail = ex.Message
                },
                "Đã xảy ra lỗi không xác định."
            );
        }
    }

    // ================== URL BUILDER ==================

    private static string BuildUrl(string url, object? query)
    {
        if (query == null)
            return url;

        var dict = query
            .GetType()
            .GetProperties()
            .Where(p => p.GetValue(query) != null)
            .ToDictionary(
                p => p.Name,
                p => p.GetValue(query)!.ToString() ?? string.Empty
            );

        if (!dict.Any())
            return url;

        return QueryHelpers.AddQueryString(url, dict);
    }

    // ================== ERROR HANDLING ==================

    private static async Task<ApiResult<T>> HandleErrorAsync<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        var raw = await response.Content.ReadAsStringAsync();

        // 1. Thử parse đúng format ApiResult<T> (backend trả chuẩn)
        try
        {
            var apiResult = JsonSerializer.Deserialize<ApiResult<T>>(raw, _jsonOptions);
            if (apiResult != null)
            {
                // Có Error từ server → map sang message thân thiện
                if (apiResult.Error != null)
                {
                    var friendly = ClientErrorCatalog.GetMessageForApiError(apiResult.Error);

                    if (string.IsNullOrWhiteSpace(apiResult.Message))
                        apiResult.Message = friendly;
                }
                else
                {
                    // Không có Error → fallback theo HTTP status
                    if (string.IsNullOrWhiteSpace(apiResult.Message))
                        apiResult.Message = ClientErrorCatalog.GetMessageForHttpStatus(statusCode, raw);
                }

                return apiResult;
            }
        }
        catch
        {
            // không parse được, fallback bên dưới
        }

        // 2. Fallback: không parse được ApiResult => dùng HTTP + raw body
        var fallbackMessage = ClientErrorCatalog.GetMessageForHttpStatus(statusCode, raw);

        return new ApiResult<T>
        {
            Success = false,
            Message = fallbackMessage,
            Error = new ApiError
            {
                Code = $"Http.{statusCode}",
                Category = statusCode switch
                {
                    (int)HttpStatusCode.Unauthorized => "Unauthorized",
                    (int)HttpStatusCode.Forbidden => "Forbidden",
                    (int)HttpStatusCode.NotFound => "NotFound",
                    _ => "HttpError"
                },
                Detail = raw.Truncate(300)
            }
        };
    }
}

