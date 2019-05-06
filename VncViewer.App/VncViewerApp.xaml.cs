using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VncViewer.App.Core;

namespace VncViewer.App
{
    public partial class VncViewerApp : Application
    {
        private MainWindow mainWindow;
        private Config config;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            config = ConfigManager.ReadLocalConfig();

            if (config == null)
            {
                MessageBox.Show("No local config found, please use VncViewer.App.Config.", "VncViewer", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
            }
            else
            {
                mainWindow = new MainWindow(config);
                ResizeMainWindow();
                mainWindow.ShowDialog();
                SaveWindowState();
            }
        }

        private void ResizeMainWindow()
        {
            if(config.IsFullScreen)
            {
                mainWindow.WindowState = WindowState.Maximized;
            }
            else if(config.WindowTop >= 0 &&
                config.WindowLeft >=0 &&
                config.WindowHeight > 0 &&
                config.WindowWidth > 0)
            {
                mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                mainWindow.Width = config.WindowWidth;
                mainWindow.Height = config.WindowHeight;
                mainWindow.Top = config.WindowTop;
                mainWindow.Left = config.WindowLeft;
            }
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
