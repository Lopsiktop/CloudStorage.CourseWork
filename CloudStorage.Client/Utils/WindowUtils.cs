using System.Windows;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace CloudStorage.Client.Utils;

public static class WindowUtils
{
    public static Window RootWindow => App.Current.MainWindow;
    public static Window? ActiveWindow => App.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

    public static void ShowRootWindow<T>()
        where T : Window, new()
    {
        ActiveWindow?.Hide();
        var window = new T();
        window.Owner = ActiveWindow;
        window.Closing += (s, e) =>
        {
            RootWindow.Close();
        };
        window.Show();
    }

    public static void ShowDialogWindow<T>()
        where T : Window, new()
    {
        var window = new T();
        window.Owner = ActiveWindow;
        window.ShowDialog();
    }
}
