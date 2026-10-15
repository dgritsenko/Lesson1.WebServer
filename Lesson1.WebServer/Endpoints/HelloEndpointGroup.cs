namespace Lesson1.WebServer.Endpoints;

public sealed class HelloEndpointGroup : IEndpointGroup
{
    private const string Greeting = "Привет! Сервер работает.";

    public string Name => "Приветствие";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        api.MapGroup("/hello")
            .WithTags(Name)
            .MapGet("/", static () => Results.Text(Greeting));
    }
}
