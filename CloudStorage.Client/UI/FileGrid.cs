using System.Windows.Controls;
using System.Windows;
using CloudStorage.Client.Models;

namespace CloudStorage.Client.UI;

public class FileGrid : Grid
{
    public static readonly DependencyProperty FileModelProperty = DependencyProperty.Register
        (
            nameof(FileModel),
            typeof(ReturnFileDto),
            typeof(FileGrid)
        );

    public ReturnFileDto FileModel
    {
        get => (ReturnFileDto)GetValue(FileModelProperty);
        set => SetValue(FileModelProperty, value);
    }
}
