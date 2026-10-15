using Lesson1.WebServer.Contracts;

namespace Lesson1.WebServer.Services;

public interface ITaskStore
{
    // Задачи
    Task<List<TaskDto>> GetPageAsync(int? categoryId, string? sortBy, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(int? categoryId, string? search, CancellationToken ct = default);
    Task<List<TaskDto>> SearchAsync(string query, CancellationToken ct = default);
    Task<TaskDto?> FindAsync(int id, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(string title, int? categoryId, CancellationToken ct = default);
    Task<TaskDto?> UpdateAsync(int id, UpdateTaskDto request, CancellationToken ct = default);
    Task<TaskDto?> ToggleDoneAsync(int id, bool isDone, CancellationToken ct = default);
    Task<bool> RemoveAsync(int id, CancellationToken ct = default);

    // Категории
    Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default);
    Task<Category?> FindCategoryAsync(int id, CancellationToken ct = default);
    Task<Category> CreateCategoryAsync(string name, CancellationToken ct = default);
    Task<Category?> UpdateCategoryAsync(int id, string name, CancellationToken ct = default);
    Task<bool> RemoveCategoryAsync(int id, CancellationToken ct = default);

    // Статистика
    Task<List<CategoryStats>> GetStatsAsync(CancellationToken ct = default);
}

/// <summary>
/// Модель статистики по категории для страницы /Stats.
/// </summary>
public sealed record CategoryStats(int CategoryId, string CategoryName, int TotalTasks, int DoneTasks);