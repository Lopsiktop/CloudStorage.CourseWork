using CloudStorage.Client.Models;
using CloudStorage.Client.UI.Modals;
using CloudStorage.Client.Utils;
using CloudStorage.Client.ViewModels;
using CloudStorage.Client.Views;
using System.Windows.Controls;
using System.Windows.Media;

namespace CloudStorage.Client.UI.UIHelpers;

public static class Property
{
    public static void ShowProperty(PropertyViewModel viewModel)
    {
        var prop = new PropertyWindow();
        prop.Owner = WindowUtils.ActiveWindow;
        prop.DataContext = viewModel;

        var grid = WindowUtils.ActiveWindow?.Content as Grid;
        if (grid != null)
        {
            var border = new Grid();
            border.Background = Brushes.LightGray;
            border.Opacity = 0.5;
            Grid.SetZIndex(border, 1000);
            grid.Children.Add(border);

            prop.ShowDialog();

            grid.Children.Remove(border);
            return;
        }

        return;
    }
}
