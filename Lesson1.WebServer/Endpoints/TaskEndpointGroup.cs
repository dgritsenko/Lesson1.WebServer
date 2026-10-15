using System.ComponentModel.DataAnnotations;
using Lesson1.WebServer.Contracts;
using Lesson1.WebServer.Infrastructure;
using Lesson1.WebServer.Services;

namespace Lesson1.WebServer.Endpoints;

/// <summary>
/// Группа эндпоинтов /api/tasks — CRUD + пагинация + поиск + категории.
/// Все ошибки возвращаются в формате: { error: { message, code } }.
/// ITaskStore разрешается из RequestServices (Scoped) в каждом запросе.
/// </summary>
public sealed class TaskEndpointGroup : IEndpointGroup
{
    public string Name => "Задачи";

    private const int DefaultPageSize = 5;

    private static object Err(string message, string? code = null)
        => new { error = new ApiError(message, code) };

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/tasks").WithTags(Name);

        group.MapGet("/", (HttpContext ctx, int? categoryId, string? sortBy, int page, int pageSize)
            => List(ctx, categoryId, sortBy, page, pageSize));

        group.MapGet("/{id:int}", (HttpContext ctx, int id)
            => Find(ctx, id));

        group.MapPost("/", (HttpContext ctx, CreateTaskDto? request)
            => Create(ctx, request));

        group.MapPut("/{id:int}", (HttpContext ctx, int id, UpdateTaskDto? request)
            => Update(ctx, id, request));

        group.MapPut("/{id:int}/done", (HttpContext ctx, int id, UpdateTaskDto? request)
            => ToggleDone(ctx, id, request));

        group.MapDelete("/{id:int}", (HttpContext ctx, int id)
            => Delete(ctx, id));

        group.MapGet("/search", (HttpContext ctx, string? q)
            => Search(ctx, q));

        group.MapGet("/categories", async (HttpContext ctx) => await GetCategories(ctx));
    }

    private ITaskStore GetStore(HttpContext context)
        => context.RequestServices.GetRequiredService<ITaskStore>();

    private async Task<IResult> List(HttpContext context, int? categoryId, string? sortBy, int page, int pageSize)
    {
        pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
        page = page <= 0 ? 1 : page;

        var store = GetStore(context);
        var items = await store.GetPageAsync(categoryId, sortBy, page, pageSize);
        var totalCount = await store.CountAsync(categoryId, null);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return TypedResults.Ok(new PagedTasksResponse(items, page, pageSize, totalCount, totalPages));
    }

    private async Task<IResult> Find(HttpContext context, int id)
    {
        var task = await GetStore(context).FindAsync(id);
        return task is null
            ? TypedResults.NotFound(new { error = new ApiError(ApiErrorMessages.TaskNotFound, ApiErrorCodes.TaskNotFound) })
            : TypedResults.Ok(task);
    }

    private async Task<IResult> Create(HttpContext context, CreateTaskDto? request)
    {
        if (request is null)
            return TypedResults.BadRequest(new { error = new ApiError(ApiErrorMessages.InvalidParameters, ApiErrorCodes.InvalidParameters) });

        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return TypedResults.BadRequest(new { error = new ApiError(ApiErrorMessages.EmptyTaskTitle, ApiErrorCodes.EmptyTaskTitle) });

        // Валидация DTO через DataAnnotations
        var dto = new CreateTaskDto(title, request.CategoryId);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), results, true))
            return TypedResults.BadRequest(new { error = new ApiError(results.First().ErrorMessage ?? "Ошибка валидации", ApiErrorCodes.EmptyTaskTitle) });

        // Проверка категории
        if (request.CategoryId.HasValue)
        {
            var cats = await GetStore(context).GetCategoriesAsync();
            if (!cats.Any(c => c.Id == request.CategoryId))
                return TypedResults.BadRequest(new { error = new ApiError(ApiErrorMessages.CategoryNotFound, ApiErrorCodes.CategoryNotFound) });
        }

        var created = await GetStore(context).CreateAsync(title, request.CategoryId);
        return TypedResults.Created($"/api/tasks/{created.Id}", created);
    }

    private async Task<IResult> Update(HttpContext context, int id, UpdateTaskDto? request)
    {
        if (request is null)
            return TypedResults.BadRequest(new { error = new ApiError(ApiErrorMessages.InvalidParameters, ApiErrorCodes.InvalidParameters) });

        var title = request.Title?.Trim();
        var dto = new UpdateTaskDto(title, request.IsDone, request.CategoryId);

        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), results, true))
            return TypedResults.BadRequest(new { error = new ApiError(results.First().ErrorMessage ?? "Ошибка валидации", ApiErrorCodes.EmptyTaskTitle) });

        var updated = await GetStore(context).UpdateAsync(id, dto);
        return updated is null
            ? TypedResults.NotFound(new { error = new ApiError(ApiErrorMessages.TaskNotFound, ApiErrorCodes.TaskNotFound) })
            : TypedResults.Ok(updated);
    }

    private async Task<IResult> ToggleDone(HttpContext context, int id, UpdateTaskDto? request)
    {
        var isDone = request?.IsDone ?? true;
        var updated = await GetStore(context).ToggleDoneAsync(id, isDone);
        return updated is null
            ? TypedResults.NotFound(new { error = new ApiError(ApiErrorMessages.TaskNotFound, ApiErrorCodes.TaskNotFound) })
            : TypedResults.Ok(updated);
    }

    private async Task<IResult> Delete(HttpContext context, int id)
    {
        var deleted = await GetStore(context).RemoveAsync(id);
        return deleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound(new { error = new ApiError(ApiErrorMessages.TaskNotFound, ApiErrorCodes.TaskNotFound) });
    }

    private async Task<IResult> Search(HttpContext context, string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return TypedResults.Ok(Array.Empty<TaskDto>());

        var items = await GetStore(context).SearchAsync(q.Trim());
        return TypedResults.Ok(items);
    }

    private async Task<IResult> GetCategories(HttpContext context)
    {
        var categories = await GetStore(context).GetCategoriesAsync();
        return TypedResults.Ok(categories.Select(c => new CategoryDto(c.Id, c.Name, c.Tasks.Count)));
    }
}