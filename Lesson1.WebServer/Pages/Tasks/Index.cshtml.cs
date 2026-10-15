using Lesson1.WebServer.Contracts;
using Lesson1.WebServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lesson1.WebServer.Pages.Tasks;

/// <summary>
/// Страница задач: таблица с пагинацией, фильтрацией, PRG-формами.
/// </summary>
public sealed class IndexModel(ITaskStore store) : PageModel
{
    public List<TaskDto> Items { get; private set; } = [];
    public List<Category> Categories { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 5;

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    public int TotalCount { get; private set; }
    public int TotalPages => PageSize == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id, bool isDone)
    {
        var updated = await store.ToggleDoneAsync(id, isDone);
        var statusText = isDone ? "выполненная" : "невыполненная";
        TempData["SuccessMessage"] = updated is null
            ? $"Задача #{id} не найдена"
            : $"Задача #{id} помечена как {statusText}";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await store.RemoveAsync(id);
        TempData["SuccessMessage"] = deleted
            ? $"Задача #{id} удалена"
            : $"Задача #{id} не найдена";

        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        if (CurrentPage < 1) CurrentPage = 1;
        if (PageSize < 1 || PageSize > 100) PageSize = 5;

        Items = await store.GetPageAsync(CategoryId, SortBy, CurrentPage, PageSize);
        TotalCount = await store.CountAsync(CategoryId, null);
        Categories = await store.GetCategoriesAsync();
    }
}