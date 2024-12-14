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
    }

    [RelayCommand]
    private async Task BanUnbanUser(AdminUserDto user)
    {
        if (user.IsBan)
        {
            var result = await AdminApi.Unban(user.Id);
            if(result.IsError || !result.Value)
            {
                await Notify.ShowAsync("Ошибка", "Не удалось разбанить пользователя", NotifyType.Error);
                return;
            }

            user.IsBan = false;
            await Notify.ShowAsync("Успех", "Пользователь успешно разбанен", NotifyType.Success, 2);
        }
        else
        {
            var result = await AdminApi.Ban(user.Id);
            if (result.IsError || !result.Value)
            {
                await Notify.ShowAsync("Ошибка", "Не удалось забанить пользователя", NotifyType.Error);
                return;
            }

            user.IsBan = true;
            await Notify.ShowAsync("Успех", "Пользователь успешно забанен", NotifyType.Success, 2);
        }
    }

    [RelayCommand]
    private async Task AdminUnadminUser(AdminUserDto user)
    {
        if (user.IsAdmin)
        {
            var result = await AdminApi.Unadmin(user.Id);
            if (result.IsError || !result.Value)
            {
                await Notify.ShowAsync("Ошибка", "Не удалось забрать админа", NotifyType.Error);
                return;
            }

            user.IsAdmin = false;
            await Notify.ShowAsync("Успех", "Успешно забрали админку", NotifyType.Success, 2);
        }
        else
        {
            var result = await AdminApi.Admin(user.Id);
            if (result.IsError || !result.Value)
            {
                await Notify.ShowAsync("Ошибка", "Не удалось выдать админку", NotifyType.Error);
                return;
            }

            user.IsAdmin = true;
            await Notify.ShowAsync("Успех", "Админка успешно выдана", NotifyType.Success, 2);
        }
    }
}
