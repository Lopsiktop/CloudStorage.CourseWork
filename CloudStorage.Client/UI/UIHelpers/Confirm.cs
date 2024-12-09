using CloudStorage.Client.UI.Modals;
using CloudStorage.Client.Utils;
using System.Windows.Controls;
using System.Windows.Media;

namespace CloudStorage.Client.UI.UIHelpers;

public static class Confirm
{
    public static bool ShowConfimation(string title, string yesbtn, string nobtn)
    {
        var conf = new Confirmation(title, yesbtn, nobtn);
        conf.Owner = WindowUtils.ActiveWindow;
        
        var grid = WindowUtils.ActiveWindow?.Content as Grid;
        if(grid != null)
        {
            var border = new Grid();
            border.Background = Brushes.LightGray;
            border.Opacity = 0.5;
            Grid.SetZIndex(border, 1000);
            grid.Children.Add(border);

            var result = conf.ShowDialog() ?? false;

            grid.Children.Remove(border);
            return result;
        }

        return conf.ShowDialog() ?? false;
    }
}
