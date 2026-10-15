namespace Lesson1.WebServer.Contracts;

public sealed record TaskDto(
    int Id,
    string Title,
    bool IsDone,
    DateTime CreatedAt,
    string? CategoryName);
