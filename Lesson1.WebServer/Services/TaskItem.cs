using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lesson1.WebServer.Services;

public sealed class TaskItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "От 3 до 100 символов")]
    [NoForbiddenWords]
    public string Title { get; set; } = "";

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CategoryId { get; set; }

    /// <summary>
    /// Навигационное свойство. Игнорируется при сериализации для избежания циклических ссылок.
    /// </summary>
    [JsonIgnore]
    public Category? Category { get; set; }
}