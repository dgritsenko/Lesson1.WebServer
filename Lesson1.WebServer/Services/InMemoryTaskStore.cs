using System.Globalization;
using Lesson1.WebServer.Contracts;

namespace Lesson1.WebServer.Services;

public sealed class InMemoryTaskStore : ITaskStore
{
    private readonly Lock _sync = new();
    private readonly List<TaskItem> _items = new();
    private readonly List<Category> _categories = new();
    private int _nextTaskId;
    private int _nextCategoryId;

    public InMemoryTaskStore()
    {
        SeedCategories();
        SeedTasks();
    }

    private void SeedCategories()
    {
        AddCategoryInternal("Работа");
        AddCategoryInternal("Учёба");
        AddCategoryInternal("Личное");
    }

    private void AddCategoryInternal(string name)
    {
        var category = new Category { Id = ++_nextCategoryId, Name = name };
        _categories.Add(category);
    }

    private void SeedTasks()
    {
        CreateInternal("Изучить ASP.NET Core", GetCategoryId("Учёба"));
        CreateInternal("Разобраться с AJAX", GetCategoryId("Учёба"));
        CreateInternal("Настроить конечные точки", GetCategoryId("Работа"));
    }

    private int? GetCategoryId(string name)
    {
        var category = _categories.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return category?.Id;
    }

    private TaskItem CreateInternal(string title, int? categoryId)
    {
        var item = new TaskItem
        {
            Id = ++_nextTaskId,
            Title = title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow,
            CategoryId = categoryId,
            Category = categoryId.HasValue
                ? _categories.FirstOrDefault(c => c.Id == categoryId)
                : null
        };
        _items.Add(item);
        return item;
    }

    public Task<List<TaskDto>> GetPageAsync(
        int? categoryId, string? sortBy, int page, int pageSize, CancellationToken ct = default)
    {
        lock (_sync)
        {
            IQueryable<TaskItem> query = _items.AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId);
            }

            query = sortBy?.ToLowerInvariant() switch
            {
                "title" => query.OrderBy(t => t.Title),
                "created" or _ => query.OrderByDescending(t => t.CreatedAt)
            };

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToList();

            return Task.FromResult(items);
        }
    }

    public Task<int> CountAsync(int? categoryId, string? search, CancellationToken ct = default)
    {
        lock (_sync)
        {
            return Task.FromResult(_items.Count(t =>
                (!categoryId.HasValue || t.CategoryId == categoryId) &&
                (string.IsNullOrEmpty(search) ||
                 t.Title.Contains(search, StringComparison.OrdinalIgnoreCase))));
        }
    }

    public Task<List<TaskDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Task.FromResult(new List<TaskDto>());
            }

            var items = _items
                .Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(ToDto)
                .ToList();

            return Task.FromResult(items);
        }
    }

    public Task<TaskDto?> FindAsync(int id, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var task = _items.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(task is null ? null : ToDto(task));
        }
    }

    public Task<TaskDto> CreateAsync(string title, int? categoryId, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var item = CreateInternal(title, categoryId);
            return Task.FromResult(ToDto(item));
        }
    }

    public Task<TaskDto?> UpdateAsync(int id, UpdateTaskDto request, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var item = _items.FirstOrDefault(t => t.Id == id);
            if (item is null)
            {
                return Task.FromResult<TaskDto?>(null);
            }

            item.Title = request.Title ?? item.Title;
            item.IsDone = request.IsDone;
            item.CategoryId = request.CategoryId;
            if (request.CategoryId.HasValue)
            {
                item.Category = _categories.FirstOrDefault(c => c.Id == request.CategoryId);
            }

            return Task.FromResult<TaskDto?>(ToDto(item));
        }
    }

    public Task<TaskDto?> ToggleDoneAsync(int id, bool isDone, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var item = _items.FirstOrDefault(t => t.Id == id);
            if (item is null)
            {
                return Task.FromResult<TaskDto?>(null);
            }

            item.IsDone = isDone;
            return Task.FromResult<TaskDto?>(ToDto(item));
        }
    }

    public Task<bool> RemoveAsync(int id, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var item = _items.FirstOrDefault(t => t.Id == id);
            if (item is null)
            {
                return Task.FromResult(false);
            }

            _items.Remove(item);
            return Task.FromResult(true);
        }
    }

    public Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default)
    {
        lock (_sync)
        {
            return Task.FromResult(new List<Category>(_categories));
        }
    }

    public Task<Category?> FindCategoryAsync(int id, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(category);
        }
    }

    public Task<Category> CreateCategoryAsync(string name, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var category = new Category { Id = ++_nextCategoryId, Name = name };
            _categories.Add(category);
            return Task.FromResult(category);
        }
    }

    public Task<Category?> UpdateCategoryAsync(int id, string name, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category is null)
            {
                return Task.FromResult<Category?>(null);
            }

            category.Name = name;
            return Task.FromResult<Category?>(category);
        }
    }

    public Task<bool> RemoveCategoryAsync(int id, CancellationToken ct = default)
    {
        lock (_sync)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category is null)
            {
                return Task.FromResult(false);
            }

            foreach (var task in _items.Where(t => t.CategoryId == id))
            {
                task.CategoryId = null;
                task.Category = null;
            }

            _categories.Remove(category);
            return Task.FromResult(true);
        }
    }

    public Task<List<CategoryStats>> GetStatsAsync(CancellationToken ct = default)
    {
        lock (_sync)
        {
            var stats = _categories.Select(c => new CategoryStats(
                c.Id,
                c.Name,
                _items.Count(t => t.CategoryId == c.Id),
                _items.Count(t => t.CategoryId == c.Id && t.IsDone))).ToList();

            var orphanTasks = _items.Count(t => t.CategoryId == null);
            var orphanDone = _items.Count(t => t.CategoryId == null && t.IsDone);
            stats.Add(new CategoryStats(0, "Без категории", orphanTasks, orphanDone));

            return Task.FromResult(stats);
        }
    }

    private static TaskDto ToDto(TaskItem item) =>
        new(
            item.Id,
            item.Title,
            item.IsDone,
            item.CreatedAt,
            item.Category?.Name ?? "Без категории");
}