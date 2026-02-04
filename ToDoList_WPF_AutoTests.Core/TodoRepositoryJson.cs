using System.Text.Json;
using ToDoList_WPF_AutoTests.Core.Models;

namespace ToDoList_WPF_AutoTests.Core;

public class TodoRepositoryJson : ITodoRepository
{
    private readonly string _filePath;
    private readonly List<TodoItem> _items = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public TodoRepositoryJson(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Load();
    }

    public IReadOnlyList<TodoItem> GetAll() => _items.AsReadOnly();

    public void Add(TodoItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
        Save();
    }

    public void Remove(Guid id)
    {
        var index = _items.FindIndex(x => x.Id == id);
        if (index >= 0)
        {
            _items.RemoveAt(index);
            Save();
        }
    }

    public void Update(TodoItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        var index = _items.FindIndex(x => x.Id == item.Id);
        if (index >= 0)
        {
            _items[index] = item;
            Save();
        }
    }

    private void Load()
    {
        _items.Clear();
        if (!File.Exists(_filePath))
            return;

        try
        {
            var json = File.ReadAllText(_filePath);
            var loaded = JsonSerializer.Deserialize<List<TodoItem>>(json);
            if (loaded != null)
                _items.AddRange(loaded);
        }
        catch
        {
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_items, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
