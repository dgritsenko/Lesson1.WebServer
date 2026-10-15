using Lesson1.WebServer.Services;

namespace Lesson1.WebServer.Endpoints;

public sealed class ServerInfoEndpointGroup(IServerInfoProvider serverInfo) : IEndpointGroup
{
    public string Name => "Информация о сервере";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        api.MapGroup("/server-info")
            .WithTags(Name)
            .MapGet("/", () => serverInfo.GetInfo());
    }
}
