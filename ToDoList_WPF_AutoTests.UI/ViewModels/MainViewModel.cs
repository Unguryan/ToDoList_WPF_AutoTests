using System.Collections.ObjectModel;
using System.Windows.Input;
using ToDoList_WPF_AutoTests.Core;
using ToDoList_WPF_AutoTests.Core.Models;

namespace ToDoList_WPF_AutoTests.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ITodoRepository _repository;
    private string _newItemTitle = string.Empty;
    private TodoItem? _selectedItem;

    public MainViewModel(ITodoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        Items = new ObservableCollection<TodoItem>();

        AddCommand = new RelayCommand(_ => Add(), _ => !string.IsNullOrWhiteSpace(NewItemTitle));
        RemoveCommand = new RelayCommand(_ => Remove(), _ => SelectedItem != null);
        ToggleCompleteCommand = new RelayCommand(_ => ToggleComplete(), _ => SelectedItem != null);

        LoadItems();
    }

    public ObservableCollection<TodoItem> Items { get; }

    public string NewItemTitle
    {
        get => _newItemTitle;
        set
        {
            if (SetProperty(ref _newItemTitle, value ?? string.Empty))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public TodoItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand AddCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand ToggleCompleteCommand { get; }

    private void LoadItems()
    {
        Items.Clear();
        foreach (var item in _repository.GetAll())
            Items.Add(item);
    }

    private void Add()
    {
        var title = NewItemTitle.Trim();
        if (string.IsNullOrEmpty(title)) return;

        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _repository.Add(item);
        Items.Add(item);
        NewItemTitle = string.Empty;
    }

    private void Remove()
    {
        if (SelectedItem == null) return;
        _repository.Remove(SelectedItem.Id);
        Items.Remove(SelectedItem);
        SelectedItem = null;
    }

    private void ToggleComplete()
    {
        if (SelectedItem == null) return;
        SelectedItem.IsCompleted = !SelectedItem.IsCompleted;
        _repository.Update(SelectedItem);
        OnPropertyChanged(nameof(SelectedItem));
    }
}
