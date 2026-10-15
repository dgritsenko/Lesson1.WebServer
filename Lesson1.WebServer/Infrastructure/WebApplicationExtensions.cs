using Lesson1.WebServer.Endpoints;

namespace Lesson1.WebServer.Infrastructure;

public static class WebApplicationExtensions
{
    public static WebApplication MapLessonApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        foreach (var group in app.Services.GetServices<IEndpointGroup>())
        {
            group.MapEndpoints(api);
        }

        app.MapRazorPages();

        return app;
    }
}