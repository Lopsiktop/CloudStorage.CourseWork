using CloudStorage.Client.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CloudStorage.Client.Converters;

public class TypeToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var type = (NodeType)value;
        if (type == NodeType.Disk)
            return new BitmapImage(new Uri("pack://application:,,,/CloudStorage.Client;component/Resources/Images/disk.png"));
        else
            return new BitmapImage(new Uri("pack://application:,,,/CloudStorage.Client;component/Resources/Images/folder.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
