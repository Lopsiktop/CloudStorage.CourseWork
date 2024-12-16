using CommunityToolkit.Mvvm.ComponentModel;
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
        var exists = App.Current.Windows.OfType<T>().FirstOrDefault();
        if (exists != null)
        {
            exists.Closing -= Window_Closing;
            exists.Close();
        }

        var window = new T();
        window.Owner = ActiveWindow;
        window.Closing += Window_Closing;
        window.Show();
    }

    private static void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        RootWindow.Close();
    }

    public static void ShowDialogWindow<T>()
        where T : Window, new()
    {
        var window = new T();
        window.Owner = ActiveWindow;
        window.ShowDialog();
    }

    public static void ShowDialogWindow<T>(ObservableObject viewModel, Func<Task> Loaded)
        where T : Window, new()
    {
        var window = new T();
        window.Owner = ActiveWindow;
        window.DataContext = viewModel;
        window.Loaded += async (s, e) =>
        {
            await Loaded.Invoke();
        };
        window.ShowDialog();
    }

    public static void ShowDialogWindow<T>(ObservableObject viewModel)
        where T : Window, new()
    {
        var window = new T();
        window.Owner = ActiveWindow;
        window.DataContext = viewModel;
        window.ShowDialog();
    }

    public static void ReturnRootWindow()
    {
        ActiveWindow?.Hide();
        RootWindow.Show();
    }
}
