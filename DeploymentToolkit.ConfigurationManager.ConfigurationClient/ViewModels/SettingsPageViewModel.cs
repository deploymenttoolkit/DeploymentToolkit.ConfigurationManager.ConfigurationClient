using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using FluentResults;

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

        private readonly IWindowsManagementInstrumentationConnection _windowsRemoteManagementClient;

        public SettingsPageViewModel(IWindowsManagementInstrumentationConnection windowsRemoteManagementClient)
        {
            _windowsRemoteManagementClient = windowsRemoteManagementClient;
        }

        [RelayCommand]
        private void TestConnect()
        {
            Result result;

            if(CredentialsEnabled)
            {
                result = _windowsRemoteManagementClient.Connect(Host, Username, Password, !AllowUnencryptedConnections);
            }
            else
            {
                result = _windowsRemoteManagementClient.Connect(Host, encrypted: !AllowUnencryptedConnections) ;
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
            _windowsRemoteManagementClient.Disconnect();
            IsWMIConnected = _windowsRemoteManagementClient.IsConnected;
        }
    }
}
