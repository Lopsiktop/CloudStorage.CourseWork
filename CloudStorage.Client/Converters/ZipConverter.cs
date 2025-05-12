using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace CloudStorage.Client.Converters;

public class ZipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = value.ToString();
        var extension = Path.GetExtension(name)!.ToLower();

        if (extension == ".zip")
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
