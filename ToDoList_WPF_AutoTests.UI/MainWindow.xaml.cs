using System.IO;
using System.Windows;
using System.Windows.Input;
using ToDoList_WPF_AutoTests.Core;
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

    private void NewItemTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainViewModel vm)
        {
            if (vm.AddCommand.CanExecute(null))
                vm.AddCommand.Execute(null);
            e.Handled = true;
        }
    }
}
