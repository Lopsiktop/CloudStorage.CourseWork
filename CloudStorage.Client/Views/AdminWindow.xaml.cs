using CloudStorage.Client.ViewModels;
using System.Windows;

namespace CloudStorage.Client.Views;

public partial class AdminWindow : Window
{
    private readonly AdminViewModel viewModel;
    public AdminWindow()
    {
        InitializeComponent();
        viewModel = new AdminViewModel();
        this.DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await viewModel.Load();
    }
}
