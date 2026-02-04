using System.IO;
using System.Windows;
using System.Windows.Controls;
using ToDoList_WPF_AutoTests.Core;
using ToDoList_WPF_AutoTests.Core.Models;
using ToDoList_WPF_AutoTests.UI.ViewModels;

namespace ToDoList_WPF_AutoTests.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ToDoList",
            "todos.json");
        var repository = new TodoRepositoryJson(appDataPath);
        DataContext = new MainViewModel(repository);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddItemWindow { Owner = this };
        if (dialog.ShowDialog() != true || DataContext is not MainViewModel vm)
            return;

        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = dialog.ItemTitle,
            Description = dialog.ItemDescription,
            Deadline = dialog.ItemDeadline,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        vm.AddItem(item);
    }

}
