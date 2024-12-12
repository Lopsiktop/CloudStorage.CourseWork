using CloudStorage.Client.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudStorage.Client.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    public AdminViewModel()
    {
        
    }

    public async Task Load()
    {

    }

    [RelayCommand]
    private void Logout()
    {
        SessionHandler.LogoutSession();
        WindowUtils.ReturnRootWindow();
    }
}
