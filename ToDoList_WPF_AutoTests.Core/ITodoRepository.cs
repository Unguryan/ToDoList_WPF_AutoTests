using ToDoList_WPF_AutoTests.Core.Models;

namespace ToDoList_WPF_AutoTests.Core;

public interface ITodoRepository
{
    IReadOnlyList<TodoItem> GetAll();
    void Add(TodoItem item);
    void Remove(Guid id);
    void Update(TodoItem item);
}
