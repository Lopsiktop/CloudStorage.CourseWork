using CloudStorage.Client.ViewModels;
using System.Windows;

namespace CloudStorage.Client.Views;

public partial class HistoryWindow : Window
{
    private readonly HistoryViewModel viewModel;

    public HistoryWindow()
    {
        InitializeComponent();
        viewModel = new HistoryViewModel();
        this.DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await viewModel.Load();
    }
}
