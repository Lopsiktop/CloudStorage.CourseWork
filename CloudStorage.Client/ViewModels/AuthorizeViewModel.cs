using CloudStorage.Client.Models;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudStorage.Client.ViewModels;

public partial class AuthorizeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _Title = "Авторизация";

    [ObservableProperty]
    private string _Login;

    [ObservableProperty]
    private string _Password;

    [ObservableProperty]
    private string _RegisterLogin;

    [ObservableProperty]
    private string _RegisterPassword;

    [ObservableProperty]
    private string _RegisterRetryPassword;

    [RelayCommand]
    private async void LoginMethod()
    {
        var result = await ApiHelper.LoginAsync(new LoginModel(_Login, _Password));
        if (result.IsError)
        {
            await Notify.ShowAsync("Авторизация", "Неверный логин или пароль", NotifyType.Error);
            return;
        }

        if (!result.Value)
        {
            await Notify.ShowAsync("Авторизация", "Ошибка сервера, попробуйте чуть позже", NotifyType.Error);
            return;
        }
    }

    [RelayCommand]
    private async void RegisterMethod()
    {
        if(_RegisterPassword != _RegisterRetryPassword)
        {
            await Notify.ShowAsync("Регистрация", "Пароли не совпадают", NotifyType.Error);
            return;
        }

        var result = await ApiHelper.RegisterAsync(new LoginModel(_RegisterLogin, _RegisterPassword));
        if (result.IsError)
        {
            await Notify.ShowAsync("Регистрация", "Данный логин занят", NotifyType.Error);
            return;
        }

        if (!result.Value)
        {
            await Notify.ShowAsync("Регистрация", "Ошибка сервера, попробуйте чуть позже", NotifyType.Error);
            return;
        }
    }
}
