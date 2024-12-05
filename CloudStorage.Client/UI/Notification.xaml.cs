using System.Windows.Controls;
using System.Windows.Media;

namespace CloudStorage.Client.UI
{
    public partial class Notification : UserControl
    {
        public Notification(string Title, string Message, Color color)
        {
            InitializeComponent();

            TitleBox.Text = Title;
            MessageBox.Text = Message;
            MainBorder.Background = new SolidColorBrush(color);
        }
    }
}
