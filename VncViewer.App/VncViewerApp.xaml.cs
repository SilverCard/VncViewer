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
                mainWindow.Show();
            }
        }
    }
}
