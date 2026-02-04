using System.Windows;

namespace ToDoList_WPF_AutoTests.UI;

public partial class AddItemWindow : Window
{
    public AddItemWindow()
    {
        InitializeComponent();
        DeadlineDatePicker.SelectedDate = DateTime.Today;
    }

    public string ItemTitle { get; private set; } = string.Empty;
    public string ItemDescription { get; private set; } = string.Empty;
    public DateTime? ItemDeadline { get; private set; }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ItemTitle = TitleTextBox.Text?.Trim() ?? string.Empty;
        ItemDescription = DescriptionTextBox.Text?.Trim() ?? string.Empty;
        ItemDeadline = DeadlineDatePicker.SelectedDate;

        if (string.IsNullOrWhiteSpace(ItemTitle))
        {
            MessageBox.Show("Please enter a title.", "Add Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!DeadlineDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Please select a deadline date.", "Add Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
