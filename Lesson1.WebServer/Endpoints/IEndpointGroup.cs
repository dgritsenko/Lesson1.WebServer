namespace Lesson1.WebServer.Endpoints;

public interface IEndpointGroup
{
    string Name { get; }

    void MapEndpoints(IEndpointRouteBuilder api);
}
