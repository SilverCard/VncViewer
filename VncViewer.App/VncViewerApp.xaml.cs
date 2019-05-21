using System;
using System.Security.Cryptography;
using System.Windows;
using VncViewer.App.Core;
using VncViewer.App.Core.WindowPlacement;
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
            mainWindow.ShowDialog();
            SaveWindowState();
        }     

        private void SaveWindowState()
        {
           ConfigManager.SaveToLocalConfig(config);
        }
    }
}
