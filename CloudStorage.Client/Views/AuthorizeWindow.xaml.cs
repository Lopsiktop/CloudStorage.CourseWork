using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CloudStorage.Client.ViewModels;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CloudStorage.Client;

public partial class MainWindow : Window
{
    private readonly AuthorizeViewModel viewModel;
    public MainWindow()
    {
        InitializeComponent();
        this.viewModel = new AuthorizeViewModel();
        this.DataContext = this.viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await viewModel.Loaded();
    }
}