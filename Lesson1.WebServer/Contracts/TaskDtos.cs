using System.ComponentModel.DataAnnotations;
using Lesson1.WebServer.Services;

namespace Lesson1.WebServer.Contracts;

public sealed record CreateTaskDto(
    [Required(ErrorMessage = "Название задачи обязательно")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
    [NoForbiddenWords]
    string? Title,
    int? CategoryId);

public sealed record UpdateTaskDto(
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
    [NoForbiddenWords]
    string? Title,
    bool IsDone,
    int? CategoryId);

public sealed record CategoryDto(int Id, string Name, int TaskCount);

public sealed record PagedTasksResponse(
    IReadOnlyList<TaskDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
