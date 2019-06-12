using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using VncViewer.App.Core;
using VncViewer.App.Cultures;

#pragma warning disable CA1305 // Specify IFormatProvider

namespace VncViewer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {     
        public Config Config { get; private set; }
        public Boolean IsFullScreen { get; private set; }
        public WindowState LastWindowState { get; private set; }

        public MainWindow(Config config)
        {
            Config = config;
            IsFullScreen = false;
            InitializeComponent();
        }

        public void OnConnected()
        {
            SetTitle($"{vvc.Hostname} - VncViewer");
        }

        public async Task HandleConnectionFailed(Exception e)
        {
            for (int i = 10; i >= 1; i--)
            {
                vvc.ShowLabelText($"{e.Message}\r\n" + String.Format(Strings.TryingConnect, i));
                await Task.Delay(1000).ConfigureAwait(true);
            }
        }

        public async Task Connect()
        {
            String vncPassword;
            try
            {
                vncPassword = Config.GetPassword();
            }
            catch (CryptographicException)
            {
                MessageBox.Show(Strings.PasswordFromFileReadFailed, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

#pragma warning disable CA1031 // Do not catch general exception types
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
#pragma warning restore CA1031 // Do not catch general exception types
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
            Config.WindowPlacement = Core.WindowPlacement.WindowPlacementUtils.GetWpfWindowPlacement(this);
        }

        private void FullScreen()
        {
            if (!IsFullScreen)
            {
                LastWindowState = this.WindowState;
                this.WindowState = WindowState.Maximized;

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
                this.WindowState = LastWindowState;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.ResizeMode = ResizeMode.CanResize;
                IsFullScreen = !IsFullScreen;
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F11)
            {
                FullScreen();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vvc.OnDisconnected += VncConnectionLost;
            await Connect().ConfigureAwait(true);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (Config.WindowPlacement.HasValue)
            {
                var wp = Config.WindowPlacement.Value;

                if (wp.showCmd == Core.WindowPlacement.WindowPlacementUtils.SW_SHOWMINIMIZED)
                    wp.showCmd = Core.WindowPlacement.WindowPlacementUtils.SW_SHOWNORMAL;

                Core.WindowPlacement.WindowPlacementUtils.SetWpfWindowPlacement(this, wp);
            }
        }
    }
}
