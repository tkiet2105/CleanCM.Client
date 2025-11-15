using System.Globalization;

namespace CleanCCM.API.Middleware;

public class LocalizationMiddleware
{
    private readonly RequestDelegate _next;

    public LocalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var language = context.Request.Headers["Accept-Language"].FirstOrDefault() ?? "vi";

        // Lấy language code đầu tiên (vd: "vi-VN" -> "vi")
        var culture = language.Split(',').First().Split('-').First();

        var supportedCultures = new[] { "vi", "en" };
        if (!supportedCultures.Contains(culture))
        {
            culture = "vi"; // Default
        }

        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        await _next(context);
    }
}