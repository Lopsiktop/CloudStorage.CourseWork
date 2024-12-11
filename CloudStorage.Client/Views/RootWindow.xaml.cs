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

            if (viewModel.TreeValue.Type == NodeType.Disk)
                DiskTab.IsSelected = true;
            else if(viewModel.TreeValue.Type == NodeType.Trash)
                BinTab.IsSelected = true;
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

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.DeleteFile(parameter);
        }

        private async void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.MoveDirToBin(parameter);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            parameter.IsEditing = true;
            viewModel.OldFile = new ReturnFileDto(parameter.Id, parameter.Name, parameter.Size);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            parameter.IsEditing = true;
            viewModel.OldDir = new ReturnDirDto(parameter.DirId, parameter.DirName);
        }

        private async void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.DownloadFile(parameter);
        }

        private async void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.DownloadDirectory(parameter);
        }
    }
}
