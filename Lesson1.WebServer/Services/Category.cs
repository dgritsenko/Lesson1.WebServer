namespace Lesson1.WebServer.Services;

public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<TaskItem> Tasks { get; set; } = new();
}
