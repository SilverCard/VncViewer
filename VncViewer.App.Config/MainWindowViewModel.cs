using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VncViewer.App.Config
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {    
        private String host;
        private int port;
        private String password;
        private Boolean isEnabled;

        public Action<MainWindowViewModel> SaveAction { get; set; }
        public Action<MainWindowViewModel> TestAction { get; set; }

        public String Host {
            get => host;
            set
            {
                host = value;
                OnPropertyRaised();
                IsValidChanged();
            }
        }

        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyRaised();
                IsValidChanged();
            }
        }

        public String Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyRaised();
                IsValidChanged();
            }
        }

        public Boolean IsValid
        {
            get
            {
                return !String.IsNullOrEmpty(Host) &&
                !String.IsNullOrEmpty(Password) &&
                Port >= 1 && Port <= 65535;
            }
        }

        public Boolean IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyRaised();
            }
        }

        private void IsValidChanged() => OnPropertyRaised(nameof(IsValid));

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand TestCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyRaised([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        public MainWindowViewModel()
        {
            SaveCommand = new RelayCommand(() => this.IsValid, SaveConfig);
            TestCommand = new RelayCommand(() => this.IsValid, TestConfig);
            IsEnabled = true;
        }

        public void SaveConfig()
        {
            IsEnabled = false;
            SaveAction?.Invoke(this);
            IsEnabled = true;
        }

        public void TestConfig()
        {
            IsEnabled = false;
            TestAction?.Invoke(this);
            IsEnabled = true;
        }
    }
}
