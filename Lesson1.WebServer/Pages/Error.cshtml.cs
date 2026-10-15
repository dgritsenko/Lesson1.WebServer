using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lesson1.WebServer.Pages;

/// <summary>
/// Страница ошибки. Отображается при необработанных исключениях (не для /api/*).
/// </summary>
public sealed class ErrorModel : PageModel
{
    public void OnGet()
    {
    }
}