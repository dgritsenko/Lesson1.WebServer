using System.ComponentModel.DataAnnotations;
using Lesson1.WebServer.Contracts;
using Lesson1.WebServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lesson1.WebServer.Pages.Tasks;

/// <summary>
/// Редактирование задачи с валидацией + PRG.
/// </summary>
public sealed class EditModel(ITaskStore store) : PageModel
{
    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(100, MinimumLength = 3)]
    [NoForbiddenWords]
    public string Title { get; set; } = "";

    [BindProperty]
    public bool IsDone { get; set; }

    [BindProperty]
    public int? CategoryId { get; set; }

    public List<Category> Categories { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var task = await store.FindAsync(id);
        if (task is null)
        {
            TempData["ErrorMessage"] = $"Задача #{id} не найдена";
            return RedirectToPage("/Tasks/Index");
        }

        Id = task.Id;
        Title = task.Title;
        IsDone = task.IsDone;
        Categories = await store.GetCategoriesAsync();

        // Устанавливаем categoryId по имени категории
        var cat = Categories.FirstOrDefault(c => c.Name == task.CategoryName);
        CategoryId = cat?.Id;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await store.GetCategoriesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dto = new UpdateTaskDto(Title.Trim(), IsDone, CategoryId);
        var updated = await store.UpdateAsync(Id, dto);

        if (updated is null)
        {
            TempData["ErrorMessage"] = $"Задача #{Id} не найдена";
            return RedirectToPage("/Tasks/Index");
        }

        TempData["SuccessMessage"] = $"Задача «{updated.Title}» обновлена";
        return RedirectToPage("/Tasks/Index");
    }
}