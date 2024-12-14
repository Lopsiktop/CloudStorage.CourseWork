using CloudStorage.Client.Attributes;
using CloudStorage.Client.Models;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CloudStorage.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

namespace CloudStorage.Client.ViewModels;

public partial class AuthorizeViewModel : ObservableValidator
{
    [ObservableProperty]
    private string _Title = "Авторизация";

    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Поле не может быть пустым")]
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginMethodCommand))]
    private string _Login;

    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Поле не может быть пустым")]
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginMethodCommand))]
    private string _Password;

    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Поле не может быть пустым")]
    [MinLength(4, ErrorMessage = "Поле должно иметь хотя бы 4 символа")]
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterMethodCommand))]
    private string _RegisterLogin;

    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Поле не может быть пустым")]
    [MinLength(4, ErrorMessage = "Поле должно иметь хотя бы 4 символа")]
    [SpecificSymbols(ErrorMessage = "Пароль должен содержать хотя бы один спец символ")]
    [UpperSymbol(ErrorMessage = "Пароль должен иметь хотя бы одну заглавную букву")]
    [LowerSymbol(ErrorMessage = "Пароль должен иметь хотя бы одну строчную букву")]
    [DigitSymbol(ErrorMessage = "Пароль должен иметь хотя бы одну цифру")]
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterMethodCommand))]
    private string _RegisterPassword;

    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Поле не может быть пустым")]
    [MinLength(4, ErrorMessage = "Поле должно иметь хотя бы 4 символа")]
    [SpecificSymbols(ErrorMessage = "Пароль должен содержать хотя бы один спец символ")]
    [UpperSymbol(ErrorMessage = "Пароль должен иметь хотя бы одну заглавную букву")]
    [LowerSymbol(ErrorMessage = "Пароль должен иметь хотя бы одну строчную букву")]
    [DigitSymbol(ErrorMessage = "Пароль должен иметь хотя бы одну цифру")]
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterMethodCommand))]
    private string _RegisterRetryPassword;

    public async Task Loaded()
    {
        var token = await SessionHandler.GetSessionAsync();
        if (token != null)
        {
            ApiHelper.SetToken(token);

            var ban = await ApiHelper.CheckBan();
            if (ban.Value)
                return;

            WindowUtils.ShowRootWindow<RootWindow>();
            return;
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoginMethodExecute))]
    private async void LoginMethod()
    {
        ValidateProperty(Login, nameof(Login));
        ValidateProperty(Password, nameof(Password));
        
        if (!CanLoginMethodExecute())
            return;

        var result = await ApiHelper.LoginAsync(new LoginModel(_Login, _Password));
        if (result.IsError)
        {
            await Notify.ShowAsync("Авторизация", "Неверный логин или пароль", NotifyType.Error);
            return;
        }

        if(result.Value.IsBan)
        {
            await Notify.ShowAsync("Ошибка", "Вы были забанены", NotifyType.Error);
            return;
        }

        if (UserHandler.IsAdmin)
            WindowUtils.ShowRootWindow<AdminWindow>();
        else
            WindowUtils.ShowRootWindow<RootWindow>();
    }

    private bool CanLoginMethodExecute() => !GetErrors(nameof(Login)).Any() && !GetErrors(nameof(Password)).Any();

    [RelayCommand(CanExecute = nameof(CanRegisterMethodExecute))]
    private async void RegisterMethod()
    {
        ValidateProperty(RegisterLogin, nameof(RegisterLogin));
        ValidateProperty(RegisterPassword, nameof(RegisterPassword));
        ValidateProperty(RegisterRetryPassword, nameof(RegisterRetryPassword));

        if (!CanRegisterMethodExecute())
            return;

        if (_RegisterPassword != _RegisterRetryPassword)
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

        WindowUtils.ShowRootWindow<RootWindow>();
    }

    private bool CanRegisterMethodExecute() => !GetErrors(nameof(RegisterLogin)).Any() 
        && !GetErrors(nameof(RegisterPassword)).Any()
        && !GetErrors(nameof(RegisterRetryPassword)).Any();
}
