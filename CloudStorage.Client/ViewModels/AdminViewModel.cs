using CloudStorage.Client.Models;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CloudStorage.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CloudStorage.Client.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    public ObservableCollection<AdminUserDto> AdminUsers { get; set; } = new ObservableCollection<AdminUserDto>();

    public AdminViewModel()
    {
        
    }

    public async Task Load()
    {
        var users = await AdminApi.GetUsersAdmin();
        if (users.IsError)
        {
            await Notify.ShowAsync("Ошибка", users.Error, NotifyType.Error);
            return;
        }

        AdminUsers.Clear();
        foreach (var item in users.Value)
            AdminUsers.Add(item);
    }

    [RelayCommand]
    private void Logout()
    {
        SessionHandler.LogoutSession();
        WindowUtils.ReturnRootWindow();
    }

    [RelayCommand]
    private async Task OpenDisk(AdminUserDto user)
    {
        var token = await AdminApi.GetUserToken(user.Id);
        if (token.IsError)
        {
            await Notify.ShowAsync("Ошибка", token.Error, NotifyType.Error);
            return;
        }

        ApiHelper.SetToken(token.Value.Token);
        WindowUtils.ShowDialogWindow<RootWindow>();

        //todo: цвет кнопки на забанить разбанить
        //todo: функция бана и разбана
    }
}
