using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudStorage.Client.Models;

public partial class ReturnDirDto : ObservableObject
{
    public ReturnDirDto(int dirId, string dirName)
    {
        DirId = dirId;
        DirName = dirName;
    }
    public int DirId { get; set; }

    [ObservableProperty]
    private string _DirName;

    [ObservableProperty]
    private bool _IsEditing = false;    
}