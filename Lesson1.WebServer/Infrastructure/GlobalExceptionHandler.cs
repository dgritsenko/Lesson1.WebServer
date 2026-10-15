using System.Diagnostics;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using Lesson1.WebServer.Contracts;
using Microsoft.AspNetCore.Diagnostics;

namespace Lesson1.WebServer.Infrastructure;

public static class GlobalExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Глобальный обработчик исключений: для /api/* возвращает JSON { error: { message, code } },
    /// для остальных запросов просто возвращает 500 с текстом.
    /// </summary>
    public static void Configure(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            // Логируем исключение
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalExceptionHandler");
            logger.LogError(exception, "Необработанное исключение: {Path}", context.Request.Path);

            context.Response.Clear();

            var isApiRequest = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

            if (isApiRequest)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = $"{MediaTypeNames.Application.Json}; charset=utf-8";

                var apiError = new ApiError(
                    "Внутренняя ошибка сервера",
                    "INTERNAL_SERVER_ERROR");

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = apiError }, JsonOptions));
            }
            else
            {
                // Для страниц просто возвращаем 500 с текстом
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/html; charset=utf-8";
                
                await context.Response.WriteAsync(@$"<html><head><title>Ошибка</title></head><body>
<h1>Произошла ошибка</h1>
<p>К сожалению, при обработке вашего запроса произошла ошибка.</p>
<p>Пожалуйста, попробуйте позже или вернитесь на <a href='/' style='color: #58a6ff;'>главную страницу</a>.</p>
</body></html>");
            }
        });
    }
}