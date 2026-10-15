using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using Lesson1.WebServer.Contracts;
using Microsoft.AspNetCore.Diagnostics;

namespace Lesson1.WebServer.Infrastructure;

public static class ApplicationBuilderExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Полный конвейер middleware согласно ТЗ:
    /// 1) GlobalExceptionHandler — JSON для /api/*, /Error для страниц
    /// 2) RequestLoggingMiddleware — Stopwatch логирование
    /// 3) ApiKeyMiddleware — проверка X-Api-Key для POST/PUT/DELETE /api/tasks
    /// 4) UseDefaultFiles / UseStaticFiles
    /// 5) StatusCodePages — единый JSON-формат ошибок
    /// </summary>
    public static WebApplication UseLessonPipeline(this WebApplication app)
    {
        app.UseExceptionHandler(GlobalExceptionHandler.Configure);
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ApiKeyMiddleware>();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseStatusCodePages(WriteJsonProblemAsync);

        return app;
    }

    private static async Task WriteJsonProblemAsync(StatusCodeContext context)
    {
        var response = context.HttpContext.Response;
        if (response.HasStarted)
        {
            return;
        }

        response.ContentType = $"{MediaTypeNames.Application.Json}; charset=utf-8";

        var (message, code) = Describe(response.StatusCode);
        var apiError = new ApiError(message, code);
        await response.WriteAsync(
            JsonSerializer.Serialize(new { error = apiError }, JsonOptions));
    }

    private static (string Message, string? Code) Describe(int statusCode) => statusCode switch
    {
        400 => ("Некорректный запрос", ApiErrorCodes.InvalidParameters),
        401 => (ApiErrorMessages.Unauthorized, ApiErrorCodes.Unauthorized),
        403 => (ApiErrorMessages.Forbidden, ApiErrorCodes.Forbidden),
        404 => (ApiErrorMessages.ResourceNotFound, ApiErrorCodes.ResourceNotFound),
        405 => (ApiErrorMessages.MethodNotAllowed, ApiErrorCodes.MethodNotAllowed),
        408 => ("Время ожидания запроса истекло", null),
        415 => ("Неподдерживаемый тип содержимого", null),
        429 => ("Слишком много запросов", ApiErrorCodes.RateLimitExceeded),
        >= 500 => (ApiErrorMessages.InternalServerError, ApiErrorCodes.InternalServerError),
        _ => ($"Ошибка HTTP {statusCode}", null)
    };
}
