using Lesson1.WebServer.Contracts;

namespace Lesson1.WebServer.Services;

public sealed class EchoService : IEchoService
{
    public EchoResponse Analyze(string text) =>
        new(text, text.ToUpperInvariant(), text.Length);
}
