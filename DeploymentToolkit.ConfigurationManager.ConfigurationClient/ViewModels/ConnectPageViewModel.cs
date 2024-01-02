using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using FluentResults;
using Microsoft.UI.Xaml.Controls;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
public partial class ConnectPageViewModel : ObservableObject
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
    private bool _isConnected;
    [ObservableProperty]
    private bool _isFileConnected;
    [ObservableProperty]
    private bool _isEncryptionSupported;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TestConnectEnabled))]
    private bool _credentialsEnabled;

    [ObservableProperty]
    private string _selectedConnectionMethod = "WinRM";
    public ObservableCollection<string> ConnectionMethods = new()
    {
        //"Auto",
        "WMI",
        "WinRM"
    };

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileTestConnectEnabled))]
    private bool _fileCredentialsEnabled;

    public bool IsFileCredentialSupported => SelectedFileConnectionMethod == "SMBClient";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileTestConnectEnabled))]
    private string _fileUsername;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileTestConnectEnabled))]
    private string _filePassword;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFileCredentialSupported))]
    private string _selectedFileConnectionMethod = "Windows";
    public ObservableCollection<string> FileConnectionMethods = new()
    {
        "Windows",
        "SMBClient"
    };

    public bool FileTestConnectEnabled
    {
        get
        {
            if (FileCredentialsEnabled)
            {
                return !string.IsNullOrEmpty(FileUsername) && !string.IsNullOrEmpty(FilePassword) && !string.IsNullOrEmpty(Host);
            }
            return !string.IsNullOrEmpty(Host);
        }
    }

    private readonly LocalSettingsService _localSettingsService;
    private readonly ClientConnectionManager _clientConnectionManager;

    public ConnectPageViewModel(ClientConnectionManager clientConnectionManager, LocalSettingsService localSettingsService)
    {
        _clientConnectionManager = clientConnectionManager;
        _localSettingsService = localSettingsService;

        IsConnected = _clientConnectionManager.Connection.IsConnected;
        IsFileConnected = _clientConnectionManager.FileExplorerConnection.IsConnected;

        SelectedConnectionMethod = _localSettingsService.UserSettings.DefaultConnectionMethod.ToString();
        SelectedFileConnectionMethod = _localSettingsService.UserSettings.DefaultFileConnectionMethod.ToString();
    }

    [RelayCommand]
    private void ConnectionMethodChanged(SelectionChangedEventArgs e)
    {
        if (!Enum.TryParse<ConnectionMethod>(SelectedConnectionMethod, true, out var selectedConnectionMethod))
        {
            return;
        }

        if (_clientConnectionManager.Connection.IsConnected)
        {
            _clientConnectionManager.Connection.Disconnect();
        }

        _clientConnectionManager.SetConnectionMethod(selectedConnectionMethod);
        IsConnected = _clientConnectionManager.Connection.IsConnected;

        switch (selectedConnectionMethod)
        {
            case ConnectionMethod.Auto:
                IsEncryptionSupported = _clientConnectionManager.Connection is WindowsRemoteManagementClient;
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

        ConnectionMethodChanged(null!);

        if (CredentialsEnabled)
        {
            result = _clientConnectionManager.Connection.Connect(Host, Username, Password, !AllowUnencryptedConnections);
        }
        else
        {
            result = _clientConnectionManager.Connection.Connect(Host, encrypted: !AllowUnencryptedConnections);
        }

        IsConnected = result.IsSuccess;

        if (result.IsSuccess)
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(
                    new InAppNotificationData(
                        string.Format("Notifications/Connect_Success".GetLocalized(), Host),
                        ""
                    )
                )
            );
        }
        else
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(
                    new InAppNotificationData(
                        string.Format("Notifications/Connect_Failed".GetLocalized(), Host),
                        result.Errors[0].Message,
                        InfoBarSeverity.Error
                    )
                )
            );
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        _clientConnectionManager.Connection.Disconnect();
        IsConnected = _clientConnectionManager.Connection.IsConnected;

        WeakReferenceMessenger.Default.Send(new NotificationMessage(new InAppNotificationData("Notifications/Disconnect_Success".GetLocalized(), "")));
    }

    [RelayCommand]
    private void FileConnectionMethodChanged(SelectionChangedEventArgs e)
    {
        if (!Enum.TryParse<FileConnectionMethods>(SelectedFileConnectionMethod, true, out var selectedConnectionMethod))
        {
            return;
        }

        if (_clientConnectionManager.FileExplorerConnection.IsConnected)
        {
            _clientConnectionManager.FileExplorerConnection.Disconnect();
        }

        _clientConnectionManager.SetConnectionMethod(selectedConnectionMethod);
        IsFileConnected = _clientConnectionManager.FileExplorerConnection.IsConnected;

        if (!IsFileCredentialSupported)
        {
            FileCredentialsEnabled = false;
        }
    }

    [RelayCommand]
    private void FileTestConnect()
    {
        if (CredentialsEnabled)
        {
            _clientConnectionManager.FileExplorerConnection.Connect(Host, FileUsername, FilePassword);
        }
        else
        {
            _clientConnectionManager.FileExplorerConnection.Connect(null, null, null);
        }

        IsFileConnected = _clientConnectionManager.FileExplorerConnection.IsConnected;

        if (IsFileConnected)
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(
                    new InAppNotificationData(
                        string.Format("Notifications/Connect_Success".GetLocalized(), Host),
                        ""
                    )
                )
            );
        }
        else
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(
                    new InAppNotificationData(
                        string.Format("Notifications/Connect_Failed".GetLocalized(), Host),
                        "",
                        InfoBarSeverity.Error
                    )
                )
            );
        }
    }

    [RelayCommand]
    private void FileDisconnect()
    {
        _clientConnectionManager.FileExplorerConnection.Disconnect();
        IsFileConnected = _clientConnectionManager.FileExplorerConnection.IsConnected;

        WeakReferenceMessenger.Default.Send(new NotificationMessage(new InAppNotificationData("Notifications/Disconnect_Success".GetLocalized(), "")));
    }
}
