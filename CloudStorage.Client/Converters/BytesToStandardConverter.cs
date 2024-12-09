using System.Globalization;
using System.Windows.Data;

namespace CloudStorage.Client.Converters;

public class BytesToStandardConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var bytes = (decimal)value;
        decimal kilobytes = Math.Round(bytes / 1024, 2);
        if(kilobytes < 1000)
            return $"{kilobytes} KB";
        
        decimal megabytes = Math.Round(bytes / 1048576, 2);
        return $"{megabytes} MB";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
