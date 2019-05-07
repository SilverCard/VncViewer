using System;
using System.Security.Cryptography;
using System.Windows;
using VncViewer.App.Core;
using VncViewer.App.Cultures;

namespace VncViewer.App
{
    public partial class VncViewerApp : Application
    {
        private MainWindow mainWindow;
        private Config config;
        public const String AppName = "VncViewer";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            config = ConfigManager.ReadLocalConfig();

            if (!CheckConfig())
            {
                Shutdown();
            }
            else
            {
                StartMainWindow();
            }
        }

        public static MessageBoxResult ShowWarning(String boxText) => MessageBox.Show(boxText, AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
        public static MessageBoxResult ShowError(String boxText) => MessageBox.Show(boxText, AppName, MessageBoxButton.OK, MessageBoxImage.Error);

        private Boolean CheckConfig()
        {
            if(config == null)
            {
                ShowWarning(Strings.ConfigNotFound);
                return false;
            }

            try
            {
                _ = config.GetPassword();
            }
            catch (CryptographicException)
            {
                ShowError(Strings.PasswordFromFileReadFailed);
                return false;
            }           

            return true;
        }

        private void StartMainWindow()
        {
            mainWindow = new MainWindow(config);
            ResizeMainWindow(); 
            mainWindow.ShowDialog();
            SaveWindowState();
        }

        private void ResizeMainWindow()
        {       
            if(config.IsFullScreen)
                mainWindow.WindowState = WindowState.Maximized;

            mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            mainWindow.Width = Math.Abs(config.WindowWidth);
            mainWindow.Height = Math.Abs(config.WindowHeight);
            mainWindow.Top = Math.Abs(config.WindowTop);
            mainWindow.Left = Math.Abs(config.WindowLeft);
        }

        private void SaveWindowState()
        {
            config.IsFullScreen = mainWindow.WindowState == WindowState.Maximized;
            config.WindowHeight = mainWindow.Height;
            config.WindowWidth = mainWindow.Width;
            config.WindowLeft = mainWindow.Left;
            config.WindowTop = mainWindow.Top;
            ConfigManager.SaveToLocalConfig(config);
        }
    }
}
