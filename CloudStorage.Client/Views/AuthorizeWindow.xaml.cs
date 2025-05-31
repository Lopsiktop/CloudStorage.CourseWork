using CloudStorage.Client.ViewModels;
using MaterialDesignThemes.Wpf;
using System.Reflection;
using System.Resources;
using System.Windows;
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

	private void PasswordSwitch(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		var source = Pass1.FontFamily.Source;

		if (source.Contains("password"))
        {
			Pass1.FontFamily = new FontFamily("Roboto");
            Eye.Kind = PackIconKind.EyeOff;
		}
        else
		{
			Pass1.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#password");
			Eye.Kind = PackIconKind.Eye;
		}
	}
}