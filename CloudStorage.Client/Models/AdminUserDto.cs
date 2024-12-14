using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudStorage.Client.Models;

public partial class AdminUserDto : ObservableObject
{
    public int Id { get; set; }
    public string Login { get; set; }

    [ObservableProperty]
    private bool _IsAdmin;

    [ObservableProperty]
    private bool _IsBan;
}