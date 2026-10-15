using Lesson1.WebServer.Contracts;

namespace Lesson1.WebServer.Services;

public interface IEchoService
{
    EchoResponse Analyze(string text);
}
