using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VncViewer.App.Config
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel Model { get; private set; }

        public MainWindow()
        {
            Model = new MainWindowViewModel();
            this.DataContext = Model;
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => Model.Password = PasswordBox.Password;

    }
}
