using System.ComponentModel.DataAnnotations;
using Lesson1.WebServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lesson1.WebServer.Pages.Categories;

/// <summary>
/// Категории: CRUD через формы + PRG + TempData.
/// </summary>
public sealed class IndexModel(ITaskStore store) : PageModel
{
    public List<Category> Categories { get; private set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Новая категория")]
    public string NewCategoryName { get; set; } = "";

    [BindProperty]
    public int? EditId { get; set; }

    [BindProperty]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Новое название")]
    public string? EditName { get; set; }

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var created = await store.CreateCategoryAsync(NewCategoryName.Trim());
        TempData["SuccessMessage"] = $"Категория «{created.Name}» создана";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (EditId is null || string.IsNullOrWhiteSpace(EditName))
        {
            TempData["ErrorMessage"] = "Заполните все поля";
            return RedirectToPage();
        }

        var updated = await store.UpdateCategoryAsync(EditId.Value, EditName!.Trim());
        if (updated is null)
        {
            TempData["ErrorMessage"] = $"Категория #{EditId} не найдена";
        }
        else
        {
            TempData["SuccessMessage"] = $"Категория переименована в «{updated.Name}»";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await store.RemoveCategoryAsync(id);
        TempData["SuccessMessage"] = deleted
            ? $"Категория #{id} удалена"
            : $"Категория #{id} не найдена";

        return RedirectToPage();
    }

    private async Task LoadCategoriesAsync()
    {
        Categories = await store.GetCategoriesAsync();
    }
}