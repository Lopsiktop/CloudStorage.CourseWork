using CloudStorage.Client.Models;
using System.Windows;
using System.Windows.Controls;

namespace CloudStorage.Client.UI;

public class FolderGrid : Grid
{
    public static readonly DependencyProperty FolderModelProperty = DependencyProperty.Register
        (
            nameof(FolderModel),
            typeof(ReturnDirDto),
            typeof (FolderGrid)
        ); 

    public ReturnDirDto FolderModel
    {
        get => (ReturnDirDto)GetValue(FolderModelProperty);
        set => SetValue(FolderModelProperty, value );
    }
}
