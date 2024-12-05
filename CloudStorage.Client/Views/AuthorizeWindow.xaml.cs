using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.ViewModels;
using System.Windows;

namespace CloudStorage.Client;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new AuthorizeViewModel();
    }
}