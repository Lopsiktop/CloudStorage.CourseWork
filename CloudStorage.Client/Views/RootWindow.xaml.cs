using CloudStorage.Client.Models;
using CloudStorage.Client.UI;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

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
            else if (viewModel.TreeValue.Type == NodeType.Trash)
                BinTab.IsSelected = true;
            else if (viewModel.TreeValue.Type == NodeType.Folder)
                DiskTab.IsSelected = true;
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

            await viewModel.MoveFileToBin(parameter);
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

        private async void RefreshFileClick(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.RefreshFileFromTrash(parameter);
        }

        private async void RefreshFolderClick(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.RefreshFolderFromTrash(parameter);
        }

        private async void DeleteForeverFolderClick(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.DeleteDirectory(parameter);
        }

        private async void DeleteForeverFileClick(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.DeleteFile(parameter);
        }

        private async void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.FolderHistory(parameter);
        }

        private async void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.FileHistory(parameter);
        }

        private async void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.FileProperties(parameter);
        }

        private async void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.FolderProperties(parameter);
        }

        private async void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.ShareFile(parameter);
        }

        private async void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.ShareFolder(parameter);
        }

        private void MenuItem_Click_12(object sender, RoutedEventArgs e)
        {
            ListMode.Visibility = Visibility.Collapsed;
            FloorMode.Visibility = Visibility.Visible;
        }

        private void MenuItem_Click_13(object sender, RoutedEventArgs e)
        {
            ListMode.Visibility = Visibility.Visible;
            FloorMode.Visibility = Visibility.Collapsed;
        }

        private async void MenuItem_Click_14(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnDirDto;
            if (parameter == null)
                return;

            await viewModel.ArchiveFolder(parameter);
        }

        private async void MenuItem_Click_15(object sender, RoutedEventArgs e)
        {
            var parameter = ((MenuItem)sender).CommandParameter as ReturnFileDto;
            if (parameter == null)
                return;

            await viewModel.UnarchiveFile(parameter);
        }

        private void MoveEnter(object sender, MouseEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var nodes = viewModel.Nodes.First().Nodes; //First().Nodes returns currentDir's folders

            GetNodes(menuItem, nodes);
        }

        private void GetNodes(MenuItem menuItem, ObservableCollection<ItemNode> nodes)
        {
            bool all = true;
            foreach (MenuItem item in menuItem.Items)
            {
                var node = item.CommandParameter as ItemNode;
                if (node.Type != NodeType.Folder)
                    continue;

                var res = nodes.Any(x => x.Name == node.Name);
                if (!res)
                {
                    all = false;
                    break;
                }
            }

            var count = 0;
            foreach (var item in menuItem.Items)
                if ((item as MenuItem)!.Header.ToString() != "(эту папку)")
                    count++;

            if (count != nodes.Count)
                all = false;

            if (all)
                return;

            menuItem.Items.Clear();

            foreach (var node in nodes)
            {
                var item = new MenuItem();
                item.Header = node.Name.ToString();
                item.CommandParameter = node;
                item.PreviewMouseLeftButtonDown += MoveDown;
                menuItem.Items.Add(item);

                GetNodes(item, node.Nodes);

                if (item.Items.Count > 0)
                    item.PreviewMouseLeftButtonDown -= MoveDown;
            }

            if (nodes.Count != 0)
            {
                var item = new MenuItem();
                item.Header = "(эту папку)";

                if ((menuItem.CommandParameter is ReturnFileDto) || (menuItem.CommandParameter is ReturnDirDto))
                    item.CommandParameter = viewModel.Nodes.First();
                else
                    item.CommandParameter = menuItem.CommandParameter;

                item.PreviewMouseLeftButtonDown += MoveDown;
                menuItem.Items.Add(item);
            }
        }

        private async void MoveDown(object sender, MouseButtonEventArgs e)
        {
            var item = (MenuItem)sender;
            var node = item.CommandParameter as ItemNode;

            var parameter = GetParameter(item);
            if (parameter == null)
                return;

            if (parameter is ReturnDirDto dir)
                await viewModel.MoveDir(dir.DirId, node.DirId);
            else if (parameter is ReturnFileDto file)
                await viewModel.MoveFile(file.Id, node.DirId);
        }

        private object GetParameter(MenuItem item)
        {
            if (item == null)
                return null;

            if (item.CommandParameter is ReturnFileDto dto)
                return dto;

            if (item.CommandParameter is ReturnDirDto dir)
                return dir;

            return GetParameter(item.Parent as MenuItem);
        }
    }
}
