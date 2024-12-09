using CloudStorage.Client.Models;
using CloudStorage.Client.UI;
using CloudStorage.Client.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CloudStorage.Client.Views
{
    public partial class RootWindow : Window
    {
        private readonly RootViewModel viewModel;
        public RootWindow()
        {
            InitializeComponent();
            viewModel = new RootViewModel();
            DataContext = viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.Loaded();
        }

        private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            viewModel.TreeValue = (ItemNode)e.NewValue;
            await viewModel.TreeValueChanged();
        }

        private async void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                var folderGrid = (FolderGrid)sender;
                await viewModel.FolderDoubleClick(folderGrid.FolderModel);
            }
        }

        private async void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            await viewModel.LostFocusEditable();
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                await viewModel.LostFocusEditable();
        }

        private async void ListView_Drop(object sender, DragEventArgs e)
        {
            viewModel.IsDragging = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await viewModel.LoadFiles(files);
            }
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            viewModel.IsDragging = true;
        }

        private void ListView_DragLeave(object sender, DragEventArgs e)
        {
            viewModel.IsDragging = false;
        }
    }
}
