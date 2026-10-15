namespace Lesson1.WebServer.Contracts;

public sealed record EchoResponse(string Original, string Uppercase, int Length);
