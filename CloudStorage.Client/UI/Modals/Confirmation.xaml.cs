using System.Windows;

namespace CloudStorage.Client.UI.Modals
{
    public partial class Confirmation : Window
    {
        public Confirmation(string title, string yes, string no)
        {
            InitializeComponent();

            TitleBox.Text = title;
            YesBox.Content = yes;
            NoBox.Content = no;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
