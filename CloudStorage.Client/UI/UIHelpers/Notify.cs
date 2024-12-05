using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Timer = System.Timers.Timer;

namespace CloudStorage.Client.UI.UIHelpers;

public static class Notify
{
    public static async Task ShowAsync(string Title, string Message, NotifyType type, int seconds = 5)
    {
        var content = App.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
        if (content == null)
            return;

        var grid = content.Content as Grid;
        if (grid == null) 
            return;

        var color = type switch
        {
            NotifyType.Success => Color.FromRgb(115, 181, 115),
            NotifyType.Info => Color.FromRgb(88, 171, 195),
            NotifyType.Warning => Color.FromRgb(249, 169, 55),
            NotifyType.Error => Color.FromRgb(202, 94, 88),
        };

        var notify = new Notification(Title, Message, color);
        grid.Children.Add(notify);

        await Task.Delay(seconds * 1000);
        grid.Children.Remove(notify);
    }
}

public enum NotifyType
{
    Success,
    Info,
    Warning,
    Error
}