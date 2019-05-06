using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using VncViewer.App.Core;
using VncViewer.Vnc;

namespace VncViewer.App.Config
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Core.Config config;
        private MainWindow mainWindow;

        void Application_Startup(object sender, StartupEventArgs e)
        {
            config = ConfigManager.ReadLocalConfig();
            if(config == null)
            {
                config = new Core.Config();
            }

            mainWindow = new MainWindow();
            ConfigToViewModel();
            mainWindow.Model.SaveAction = SaveConfig;
            mainWindow.Model.TestAction = TestConfig;
            mainWindow.Show();
        }

        private void ConfigToViewModel()
        {
            var vm = mainWindow.Model;
            vm.Port = config.Port;
            vm.Host = config.Host;
        }

        private void ViewModelToConfig()
        {
            var vm = mainWindow.Model;
            config.Host = vm.Host;
            config.Port = vm.Port;
            config.SetPassword(vm.Password);
        }

        private void SaveConfig(MainWindowViewModel vm)
        {
            ViewModelToConfig();
            try
            {
                ConfigManager.SaveToLocalConfig(config);
                MessageBox.Show("Saved.", "Config", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to save.\r\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
       
        }

        private void TestConfig(MainWindowViewModel vm)
        {
            using (var client = new VncClient(config.BitsPerPixel, config.Depth))
            {
                try
                {
                    client.Connect(vm.Host, vm.Port);
                    client.Authenticate(new VncAuthenticator(vm.Password));
                    client.Disconnect();
                    MessageBox.Show("Success.", "Config", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (SocketException e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (VncSecurityException e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{e.Message}\r\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

            
        }

    }
}
