using System.Globalization;
using System.Windows.Data;

namespace CloudStorage.Client.Converters;

public class ButtonIsBanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var ban = (bool)value;
        return ban ? "Разбанить" : "Забанить";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
