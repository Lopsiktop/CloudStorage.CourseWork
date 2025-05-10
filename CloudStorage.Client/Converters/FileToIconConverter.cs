using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CloudStorage.Client.Converters;

public class FileToIconConverter : IValueConverter
{
    private BitmapImage GetIcon(string name) => new BitmapImage(new Uri(PathToIcon(name)));
    private string PathToIcon(string name) => $"pack://application:,,,/CloudStorage.Client;component/Resources/Images/{name}";

    public bool ExtContains(string extension, string search) =>
        search.Split(';').Contains(extension);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = value.ToString();
        var extension = Path.GetExtension(name)!.ToLower();
        if (extension == ".txt")
            return GetIcon("txt.png");
        else if (new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".cr2" }.Contains(extension))
            return GetIcon("pictures.png");
        else if (extension == ".xls" || extension == ".xlsx")
            return GetIcon("xlsx.png");
        else if (extension == ".pptx")
            return GetIcon("pptx.png");
        else if (extension == ".doc" || extension == ".docx")
            return GetIcon("docx-file.png");
        else if (extension == ".pdf")
            return GetIcon("pdf.png");
        else if (extension == ".csv")
            return GetIcon("csv-file.png");
        else if (ExtContains(extension, ".mp4;.avi;.mov;.mkv;.wmv;.flv;.webm;.mpeg;.mpg;.3gp;.rm;.rmvb"))
            return GetIcon("video-file.png");
        else if (ExtContains(extension, ".mp3;.wav;.flac;.aac;.ogg;.m4a;.wma;.aiff;.dsd;.opus"))
            return GetIcon("music-notes.png");
        else if (ExtContains(extension, ".zip;.rar;.7z;.tar;.gz;.bz2;.xz;.cab;.iso;.arj;.lzh;.z;.ace;.tar.gz;.tar.bz2"))
            return GetIcon("zip-folder.png");
        else
            return GetIcon("unknown360.png");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}