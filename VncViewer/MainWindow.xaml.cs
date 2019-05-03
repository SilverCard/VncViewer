using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace VncViewer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const String ConfigFilename = "Config.json";
        public Config Config { get; private set; }
        public Boolean IsFullScreen { get; private set; }

        public MainWindow()
        {
            Config = Config.ReadFromFile(ConfigFilename);

            if(Config.Password != null)
            {
                Config.ProtectPassword();
                Config.Save(ConfigFilename);
            }

            IsFullScreen = false;
        }

        public void OnConnected()
        {
            SetTitle($"{vvc.Hostname} - VncViewer");
        }

        public async Task HandleConnectionFailed(Exception e)
        {
            for (int i = 10; i >= 1; i--)
            {
                vvc.ShowLabelText($"{e.Message}\r\nTrying to connected in {i}s.");
                await Task.Delay(1000).ConfigureAwait(true);
            }
        }

        public async Task Connect()
        {
            String vncPassword;
            try
            {
                vncPassword = Config.GetUnprotectedPassword();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to get the protect password from config file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
 

            while (true)
            {
                try
                {
                    await vvc.ConnectAsync(Config.Host, Config.Port, Config.BitsPerPixel, Config.Depth).ConfigureAwait(true);
                    await vvc.VncAuthenticate(vncPassword).ConfigureAwait(true);
                    await vvc.InitializeAsync().ConfigureAwait(true);
                    OnConnected();
                    break;
                }
                catch (Exception ex)
                {
                    await HandleConnectionFailed(ex).ConfigureAwait(true);
                }
            }
        }

        private async void Window_Initialized(object sender, System.EventArgs e)
        {
            vvc.OnDisconnected += VncConnectionLost;
            await Connect().ConfigureAwait(true);
        }

        private void SetTitle(String title)
        {
            Dispatcher.Invoke(() => Title = title);
        }

        private void VncConnectionLost(object sender, EventArgs e)
        {
            _ = Dispatcher.InvokeAsync(Connect);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vvc.OnDisconnected -= VncConnectionLost;
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F11)
            {
                if (!IsFullScreen)
                {
                    // hide the window before changing window style
                    this.Visibility = Visibility.Collapsed;
                    this.WindowStyle = WindowStyle.None;
                    this.ResizeMode = ResizeMode.NoResize;
                    // re-show the window after changing style
                    this.Visibility = Visibility.Visible;


                    IsFullScreen = !IsFullScreen;
                }
                else
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.ResizeMode = ResizeMode.CanResize;

                    IsFullScreen = !IsFullScreen;
                }

            }
        }
    }
}
