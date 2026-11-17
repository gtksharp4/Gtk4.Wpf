using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppToConvert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ResultLabel.Content = "Action performed!";
        }

        private void ShowDateTimeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(System.DateTime.Now.ToString(), "Current DateTime");
        }

        private void OpenDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MainWindow { Owner = this };
            dialog.ShowDialog();
        }
    }
}
