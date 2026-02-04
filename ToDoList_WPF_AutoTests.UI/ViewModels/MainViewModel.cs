using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ToDoList_WPF_AutoTests.Core;
using ToDoList_WPF_AutoTests.Core.Models;

namespace ToDoList_WPF_AutoTests.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ITodoRepository _repository;
    private TodoItem? _selectedItem;

    public MainViewModel(ITodoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        Items = new ObservableCollection<TodoItem>();
        SelectedItems = new ObservableCollection<TodoItem>();

        RemoveCommand = new RelayCommand(_ => Remove(), _ => GetSelectedCount() > 0);
        ToggleCompleteCommand = new RelayCommand(_ => ToggleComplete(), _ => GetSelectedCount() > 0);

        LoadItems();
        SubscribeToSelectionChanges();
    }

    public ObservableCollection<TodoItem> Items { get; }
    public ObservableCollection<TodoItem> SelectedItems { get; }

    private void SubscribeToSelectionChanges()
    {
        foreach (var item in Items)
            item.PropertyChanged += Item_PropertyChanged;
        Items.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
                foreach (TodoItem item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
        };
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TodoItem.IsSelected))
        {
            SyncSelectedItems(Items.Where(i => i.IsSelected));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private int GetSelectedCount() => Items.Count(i => i.IsSelected);

    public TodoItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand RemoveCommand { get; }
    public ICommand ToggleCompleteCommand { get; }

    public void AddItem(TodoItem item)
    {
        if (item == null) return;
        _repository.Add(item);
        Items.Add(item);
    }

    public void SyncSelectedItems(IEnumerable<TodoItem> items)
    {
        var selectedSet = items as IReadOnlyCollection<TodoItem> ?? items.ToList();
        foreach (var item in Items)
            item.IsSelected = selectedSet.Contains(item);
        SelectedItems.Clear();
        foreach (var item in selectedSet)
            SelectedItems.Add(item);
        CommandManager.InvalidateRequerySuggested();
    }

    private void LoadItems()
    {
        Items.Clear();
        foreach (var item in _repository.GetAll())
            Items.Add(item);
    }

    private void Remove()
    {
        var toRemove = Items.Where(i => i.IsSelected).ToList();
        if (toRemove.Count == 0) return;
        if (!ConfirmRemove(toRemove.Count)) return;

        foreach (var item in toRemove)
        {
            _repository.Remove(item.Id);
            Items.Remove(item);
        }
        SelectedItems.Clear();
        SelectedItem = null;
    }

    private void ToggleComplete()
    {
        var toComplete = Items.Where(i => i.IsSelected).ToList();
        if (toComplete.Count == 0) return;
        if (!ConfirmComplete(toComplete.Count)) return;

        foreach (var item in toComplete)
        {
            item.IsCompleted = !item.IsCompleted;
            _repository.Update(item);
        }
        OnPropertyChanged(nameof(SelectedItem));
    }

    protected virtual bool ConfirmRemove(int count)
    {
        var message = count == 1 ? "Remove this item?" : $"Remove {count} items?";
        return MessageBox.Show(message, "Confirm remove", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    protected virtual bool ConfirmComplete(int count)
    {
        var message = count == 1 ? "Mark this item as complete?" : $"Mark {count} items as complete?";
        return MessageBox.Show(message, "Confirm complete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
