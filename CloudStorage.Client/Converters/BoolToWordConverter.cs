using System.Globalization;
using System.Windows.Data;

namespace CloudStorage.Client.Converters;

public class BoolToWordConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boo = (bool)value;
        return boo ? "Да" : "Нет";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
