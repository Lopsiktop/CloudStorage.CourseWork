using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace CloudStorage.Client.Converters;

public class BoolToVisibleDescConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = (bool)value;
        return val ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
