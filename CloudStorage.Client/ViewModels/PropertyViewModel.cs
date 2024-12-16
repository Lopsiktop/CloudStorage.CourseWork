using CloudStorage.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudStorage.Client.ViewModels;

public partial class PropertyViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _IsFile = false;

    [ObservableProperty]
    private string _Path = "CloudStorage/Dirs/Сигма/Fasfasf/Fasfgasgas/Ye643643";

    [ObservableProperty]
    private DateTime _CreatedDate = DateTime.Now;

    [ObservableProperty]
    private DateTime _ModifiedDate = DateTime.Now.AddDays(+2);

    [ObservableProperty]
    private DateTime _UploadedDate = DateTime.Now.AddDays(-1);

    [ObservableProperty]
    private decimal _Size = 102402159;

    [ObservableProperty]
    private string _Name = "Сигма askf[pfkaspfks oapfkopsafkasopfoask fpasfas";

    [ObservableProperty]
    private int _FolderContains = 4;

    [ObservableProperty]
    private int _FilesContains = 10;

    public PropertyViewModel()
    {
        
    }

    public PropertyViewModel(FileProperties property, string name)
    {
        IsFile = true;
        Name = name;
        Path = property.Path;
        CreatedDate = property.CreatedDate;
        ModifiedDate = property.ModifiedDate;
        UploadedDate = property.UploadedDate;
        Size = property.Size;
    }

    public PropertyViewModel(FolderProperties property, string name)
    {
        IsFile = false;
        Name = name;
        Path = property.Path;
        CreatedDate = property.CreatedDate;
        Size = property.Size;
        FolderContains = property.Folders;
        FilesContains = property.Files;
    }
}
