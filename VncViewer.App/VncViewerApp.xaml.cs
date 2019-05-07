using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using VncViewer.App.Core;
using VncViewer.App.Cultures;

namespace VncViewer.App
{
    public partial class VncViewerApp : Application
    {
        private MainWindow mainWindow;
        private Config config;

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

        private Boolean CheckConfig()
        {
            if(config == null)
            {
                MessageBox.Show(Strings.ConfigNotFound, "VncViewer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                _ = config.GetPassword();
            }
            catch (CryptographicException)
            {
                MessageBox.Show(Strings.PasswordFromFileReadFailed, "VncViewer", MessageBoxButton.OK, MessageBoxImage.Error);
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
