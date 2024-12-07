using CloudStorage.Client.Models;
using CloudStorage.Client.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace CloudStorage.Client.Views
{
    public partial class RootWindow : Window
    {
        public RootWindow()
        {
            InitializeComponent();
            DataContext = new RootViewModel();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await (DataContext as RootViewModel).Loaded();
        }

        private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            (DataContext as RootViewModel).TreeValue = (ItemNode)e.NewValue;
        }
    }
}
