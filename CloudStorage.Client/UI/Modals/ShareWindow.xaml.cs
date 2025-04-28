using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using System.Windows;

namespace CloudStorage.Client.UI.Modals;

public partial class ShareWindow : Window
{
    public ShareWindow(string url)
    {
        InitializeComponent();
        UrlBox.Text = url;
    }

    private async void DeleteLink_Click(object sender, RoutedEventArgs e)
    {
        var result = await ApiHelper.DeleteLink(UrlBox.Text);
        if (result.IsError)
        {
            this.Hide();
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            this.Close();
            return;
        }

        this.Hide();
        await Notify.ShowAsync("Успех", "Ссылка успешно удалена!", NotifyType.Success);
        this.Close();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}
