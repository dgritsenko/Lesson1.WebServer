using Lesson1.WebServer.Contracts;
using Lesson1.WebServer.Data;
using Microsoft.EntityFrameworkCore;

namespace Lesson1.WebServer.Services;

/// <summary>
/// Реализация хранилища задач на Entity Framework Core (SQLite).
/// Все запросы асинхронные, связанная категория подгружается через Include.
/// </summary>
public sealed class EfTaskStore(TaskDbContext db) : ITaskStore
{
    public async Task<List<TaskDto>> GetPageAsync(
        int? categoryId, string? sortBy, int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<TaskItem> query = db.Tasks.AsNoTracking().Include(t => t.Category);

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "title" => query.OrderBy(t => t.Title),
            "created" or _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return items.Select(ToDto).ToList();
    }

    public async Task<int> CountAsync(int? categoryId, string? search, CancellationToken ct = default)
    {
        IQueryable<TaskItem> query = db.Tasks.AsNoTracking();

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Title.Contains(search));
        }

        return await query.CountAsync(ct);
    }

    public async Task<List<TaskDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var items = await db.Tasks.AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.Title.Contains(query))
            .ToListAsync(ct);

        return items.Select(ToDto).ToList();
    }

    public async Task<TaskDto?> FindAsync(int id, CancellationToken ct = default)
    {
        var task = await db.Tasks.AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return task is null ? null : ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(string title, int? categoryId, CancellationToken ct = default)
    {
        var category = categoryId.HasValue
            ? await db.Categories.FindAsync([categoryId.Value], ct)
            : null;

        var item = new TaskItem
        {
            Title = title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow,
            CategoryId = categoryId,
            Category = category
        };

        db.Tasks.Add(item);
        await db.SaveChangesAsync(ct);

        return ToDto(item);
    }

    public async Task<TaskDto?> UpdateAsync(int id, UpdateTaskDto request, CancellationToken ct = default)
    {
        var item = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (item is null)
        {
            return null;
        }

        item.Title = request.Title ?? item.Title;
        item.IsDone = request.IsDone;
        item.CategoryId = request.CategoryId;

        await db.SaveChangesAsync(ct);

        await db.Entry(item).Reference(t => t.Category).LoadAsync(ct);
        return ToDto(item);
    }

    public async Task<TaskDto?> ToggleDoneAsync(int id, bool isDone, CancellationToken ct = default)
    {
        var item = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (item is null)
        {
            return null;
        }

        item.IsDone = isDone;
        await db.SaveChangesAsync(ct);

        await db.Entry(item).Reference(t => t.Category).LoadAsync(ct);
        return ToDto(item);
    }

    public async Task<bool> RemoveAsync(int id, CancellationToken ct = default)
    {
        var item = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (item is null)
        {
            return false;
        }

        db.Tasks.Remove(item);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await db.Categories.AsNoTracking()
            .Include(c => c.Tasks)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task<Category?> FindCategoryAsync(int id, CancellationToken ct = default)
    {
        return await db.Categories.AsNoTracking()
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Category> CreateCategoryAsync(string name, CancellationToken ct = default)
    {
        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(int id, string name, CancellationToken ct = default)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return null;
        }

        category.Name = name;
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<bool> RemoveCategoryAsync(int id, CancellationToken ct = default)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return false;
        }

        // DeleteBehavior.SetNull — задачи сохранятся, CategoryId станет null
        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<CategoryStats>> GetStatsAsync(CancellationToken ct = default)
    {
        var categories = await db.Categories.AsNoTracking()
            .Include(c => c.Tasks)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        // Добавляем статистику для задач без категории
        var orphanTasks = await db.Tasks.AsNoTracking()
            .Where(t => t.CategoryId == null)
            .CountAsync(ct);
        var orphanDone = await db.Tasks.AsNoTracking()
            .Where(t => t.CategoryId == null && t.IsDone)
            .CountAsync(ct);

        var stats = categories.Select(c => new CategoryStats(
            c.Id,
            c.Name,
            c.Tasks.Count,
            c.Tasks.Count(t => t.IsDone))).ToList();

        stats.Add(new CategoryStats(0, "Без категории", orphanTasks, orphanDone));

        return stats;
    }

    private static TaskDto ToDto(TaskItem item) =>
        new(
            item.Id,
            item.Title,
            item.IsDone,
            item.CreatedAt,
            item.Category?.Name ?? "Без категории");
}