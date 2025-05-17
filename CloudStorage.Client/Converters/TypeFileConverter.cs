using CloudStorage.Client.Models;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace CloudStorage.Client.Converters;

public class TypeFileConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ReturnFileDto file)
        {
            var name = file.Name;
            var extension = Path.GetExtension(name)!.ToLower();
            if (extension == ".txt")
                return "Текстовый документ";
            else if (new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".cr2" }.Contains(extension))
                return "Изображение";
            else if (extension == ".xls" || extension == ".xlsx")
                return "Excel таблица";
            else if (extension == ".pptx")
                return "Презентация";
            else if (extension == ".doc" || extension == ".docx")
                return "Word документ";
            else if (extension == ".pdf")
                return "PDF файл";
            else if (extension == ".csv")
                return "CSV файл";
            else if (FileToIconConverter.ExtContains(extension, ".mp4;.avi;.mov;.mkv;.wmv;.flv;.webm;.mpeg;.mpg;.3gp;.rm;.rmvb"))
                return "Видео файл";
            else if (FileToIconConverter.ExtContains(extension, ".mp3;.wav;.flac;.aac;.ogg;.m4a;.wma;.aiff;.dsd;.opus"))
                return "Аудио файл";
            else if (FileToIconConverter.ExtContains(extension, ".zip;.rar;.7z;.tar;.gz;.bz2;.xz;.cab;.iso;.arj;.lzh;.z;.ace;.tar.gz;.tar.bz2"))
                return "Архив";
            else
                return "Файл";
        }
        else if (value is ReturnDirDto)
            return "Папка";

        return "None";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
