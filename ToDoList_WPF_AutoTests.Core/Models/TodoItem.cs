using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ToDoList_WPF_AutoTests.Core.Models;

public class TodoItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
