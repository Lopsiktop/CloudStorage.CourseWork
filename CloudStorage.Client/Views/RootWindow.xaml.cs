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
    }
}
