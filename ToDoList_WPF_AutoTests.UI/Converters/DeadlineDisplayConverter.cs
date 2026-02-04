using System.Globalization;
using System.Windows.Data;

namespace ToDoList_WPF_AutoTests.UI.Converters;

public class DeadlineDisplayConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
            return $" (due {date:d})";
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
