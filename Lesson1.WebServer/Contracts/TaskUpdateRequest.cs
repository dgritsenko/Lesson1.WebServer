namespace Lesson1.WebServer.Contracts;

public sealed record TaskUpdateRequest(string? Title, bool? IsDone);
