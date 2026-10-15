using System.ComponentModel.DataAnnotations;
using Lesson1.WebServer.Contracts;
using Lesson1.WebServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lesson1.WebServer.Pages.Tasks;

/// <summary>
/// Создание задачи с валидацией + PRG.
/// </summary>
public sealed class CreateModel(ITaskStore store) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(100, MinimumLength = 3)]
    [NoForbiddenWords]
    public string Title { get; set; } = "";

    [BindProperty]
    public int? CategoryId { get; set; }

    public List<Category> Categories { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Categories = await store.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await store.GetCategoriesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var created = await store.CreateAsync(Title.Trim(), CategoryId);
        TempData["SuccessMessage"] = $"Задача «{created.Title}» создана (ID: {created.Id})";
        return RedirectToPage("/Tasks/Index");
    }
}