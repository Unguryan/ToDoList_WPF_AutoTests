namespace ToDoList_WPF_AutoTests.Core.Models;

public class TodoItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
