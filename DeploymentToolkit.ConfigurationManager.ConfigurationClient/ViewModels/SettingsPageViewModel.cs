using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using FluentResults;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TestConnectEnabled))]
        private string _username;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TestConnectEnabled))]
        private string _password;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TestConnectEnabled))]
        private string _host = "127.0.0.1";
        [ObservableProperty]
        private bool _allowUnencryptedConnections;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TestConnectEnabled))]
        private bool _credentialsEnabled;

        [ObservableProperty]
        private bool _isWMIConnected;
        [ObservableProperty]
        private bool _isEncryptionSupported;

        [ObservableProperty]
        private string _selectedConnectionMethod = "Auto";
        public ObservableCollection<string> ConnectionMethods = new()
        {
            "Auto",
            "WMI",
            "WinRM"
        };

        public InAppNotification Notification;

        public bool TestConnectEnabled
        {
            get
            {
                if (CredentialsEnabled)
                {
                    return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(Host);
                }
                return !string.IsNullOrEmpty(Host);
            }
        }

        private readonly IWindowsManagementInstrumentationConnection _windowsManagementInstrumentationConnection;

        public SettingsPageViewModel(IWindowsManagementInstrumentationConnection windowsRemoteManagementClient)
        {
            _windowsManagementInstrumentationConnection = windowsRemoteManagementClient;
        }

        [RelayCommand]
        private void ConnectionMethodChanged(SelectionChangedEventArgs e)
        {
            if (!Enum.TryParse<ConnectionMethod>(SelectedConnectionMethod, true, out var selectedConnectionMethod))
            {
                return;
            }

            switch(selectedConnectionMethod)
            {
                case ConnectionMethod.Auto:
                    // TODO: properly evaluate
                    IsEncryptionSupported = _windowsManagementInstrumentationConnection is WindowsRemoteManagementClient;
                    break;

                case ConnectionMethod.WMI:
                    IsEncryptionSupported = false;
                    break;

                case ConnectionMethod.WinRM:
                    IsEncryptionSupported = true;
                    break;
            }
        }

        [RelayCommand]
        private void TestConnect()
        {
            Result result;

            if(CredentialsEnabled)
            {
                result = _windowsManagementInstrumentationConnection.Connect(Host, Username, Password, !AllowUnencryptedConnections);
            }
            else
            {
                result = _windowsManagementInstrumentationConnection.Connect(Host, encrypted: !AllowUnencryptedConnections) ;
            }

            IsWMIConnected = result.IsSuccess;

            if(result.IsSuccess)
            {
                Notification.Show(new InAppNotificationData($"Successfully connected to WinRM on host {Host}", ""), 5000);
            }
            else
            {
                Notification.Show(new InAppNotificationData($"Failed to connect to WinRM on host {Host}", result.Errors[0].Message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error), 5000);
            }
        }

        [RelayCommand]
        private void Disconnect()
        {
            _windowsManagementInstrumentationConnection.Disconnect();
            IsWMIConnected = _windowsManagementInstrumentationConnection.IsConnected;
        }
    }
}
