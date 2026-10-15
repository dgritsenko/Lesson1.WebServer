namespace Lesson1.WebServer.Contracts;

public sealed record ServerInfoResponse(string ServerName, string Version, DateTime CurrentTime);
