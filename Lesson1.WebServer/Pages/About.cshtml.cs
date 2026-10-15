using Lesson1.WebServer.Contracts;
using Lesson1.WebServer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lesson1.WebServer.Pages;

/// <summary>
/// Страница "О сервере".
/// </summary>
public sealed class AboutModel(IServerInfoProvider serverInfo) : PageModel
{
    public ServerInfoResponse ServerInfo { get; private set; } = null!;

    public void OnGet()
    {
        ServerInfo = serverInfo.GetInfo();
    }
}