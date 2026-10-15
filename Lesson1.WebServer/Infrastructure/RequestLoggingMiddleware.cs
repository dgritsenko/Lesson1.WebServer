using System.Diagnostics;

namespace Lesson1.WebServer.Infrastructure;

/// <summary>
/// Middleware логирования запросов.
/// Фиксирует в консоли время, HTTP-метод, путь, статус ответа и время выполнения.
/// </summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;

            logger.LogInformation(
                "{Time:yyyy-MM-dd HH:mm:ss} | {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                DateTime.Now,
                context.Request.Method,
                context.Request.Path + context.Request.QueryString,
                context.Response.StatusCode,
                elapsed);
        }
    }
}