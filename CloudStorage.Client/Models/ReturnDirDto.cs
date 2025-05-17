using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudStorage.Client.Models;

public partial class ReturnDirDto : ObservableObject
{
    public ReturnDirDto(int dirId, string dirName, DateTime creationTime)
    {
        DirId = dirId;
        DirName = dirName;
        CreationTime = creationTime;
    }
    public int DirId { get; set; }

    [ObservableProperty]
    private string _DirName;

    [ObservableProperty]
    private bool _IsEditing = false;

    [ObservableProperty]
    private DateTime _CreationTime;
}