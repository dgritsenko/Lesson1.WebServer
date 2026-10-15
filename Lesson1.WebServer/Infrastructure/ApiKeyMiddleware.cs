using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using Lesson1.WebServer.Contracts;

namespace Lesson1.WebServer.Infrastructure;

/// <summary>
/// Middleware проверки API-ключа.
/// Защищает изменяющие методы (POST, DELETE, PUT) по пути /api/tasks.
/// Требует заголовок X-Api-Key; иначе возвращает 401 Unauthorized в едином формате ошибок.
/// </summary>
public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task InvokeAsync(HttpContext context)
    {
        // Защита только для изменяющих методов по пути /api/tasks
        var path = context.Request.Path.ToString();
        var method = context.Request.Method;

        var isProtected = path.StartsWith("/api/tasks", StringComparison.OrdinalIgnoreCase)
            && (method == HttpMethods.Post || method == HttpMethods.Delete || method == HttpMethods.Put);

        if (isProtected)
        {
            var headerName = configuration["ApiKey:HeaderName"] ?? "X-Api-Key";
            var expectedKey = configuration["ApiKey:Key"] ?? "";

            if (!context.Request.Headers.TryGetValue(headerName, out var providedKey)
                || !string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = $"{MediaTypeNames.Application.Json}; charset=utf-8";

                var error = new ApiError(ApiErrorMessages.Unauthorized, ApiErrorCodes.Unauthorized);
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error }, JsonOptions));
                return;
            }
        }

        await next(context);
    }
}