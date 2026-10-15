namespace Lesson1.WebServer.Contracts;

public sealed record ApiError(
    string Message,
    string? Code = null);
