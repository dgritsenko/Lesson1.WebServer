using Lesson1.WebServer.Data;
using Lesson1.WebServer.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрация сервисов согласно ТЗ
builder.Services.AddLessonCoreServices(builder.Configuration);
builder.Services.AddLessonEndpointGroups();

var app = builder.Build();

// 2. Авто-миграция БД при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    db.Database.Migrate();
}

// 3. Конвейер middleware согласно ТЗ:
//    - GlobalExceptionHandler (500 JSON для /api/*, показ ошибки для страниц)
//    - RequestLoggingMiddleware (Stopwatch логирование)
//    - ApiKeyMiddleware (проверка X-Api-Key для POST/PUT/DELETE /api/tasks)
//    - Static files
//    - StatusCodePages (единый JSON формат ошибок)
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();

// Убираем UseExceptionHandler и StatusCodePages
// app.UseExceptionHandler(GlobalExceptionHandler.Configure);
// app.UseStatusCodePages();

// 4. Маршрутизация: API группы + Razor Pages
app.MapLessonApi();

app.Run();