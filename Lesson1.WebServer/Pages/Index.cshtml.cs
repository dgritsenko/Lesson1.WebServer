using Lesson1.WebServer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lesson1.WebServer.Pages;

/// <summary>
/// Главная страница: SSR сводка + интерактивный клиентский SPA (AJAX / REST API) без перезагрузки страницы.
/// </summary>
public sealed class IndexModel : PageModel
{
    public int TaskCount { get; private set; }
    public int DoneCount { get; private set; }
    public int CategoryCount { get; private set; }

    public async Task OnGetAsync()
    {
        var store = HttpContext.RequestServices.GetRequiredService<ITaskStore>();
        var stats = await store.GetStatsAsync();
        TaskCount = stats.Sum(s => s.TotalTasks);
        DoneCount = stats.Sum(s => s.DoneTasks);
        CategoryCount = stats.Count(c => c.CategoryId > 0);
    }
}
