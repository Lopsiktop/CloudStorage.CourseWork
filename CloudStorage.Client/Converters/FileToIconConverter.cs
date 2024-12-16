using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CloudStorage.Client.Converters;

public class FileToIconConverter : IValueConverter
{
    private BitmapImage GetIcon(string name) => new BitmapImage(new Uri(PathToIcon(name)));
    private string PathToIcon(string name) => $"pack://application:,,,/CloudStorage.Client;component/Resources/Images/{name}";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = value.ToString();
        var extension = Path.GetExtension(name).ToLower();
        if (extension == ".txt")
            return GetIcon("txt.png");
        else if (new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" }.Contains(extension))
            return GetIcon("pictures.png");
        else
            return GetIcon("unknown360.png");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}