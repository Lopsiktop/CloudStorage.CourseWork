using System.Globalization;
using System.Windows.Data;

namespace CloudStorage.Client.Converters;

public class CreationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var date = (DateTime)value;
        return date.ToLongDateString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}