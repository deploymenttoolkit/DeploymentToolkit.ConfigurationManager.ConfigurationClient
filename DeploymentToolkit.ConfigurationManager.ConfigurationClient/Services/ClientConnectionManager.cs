using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using Microsoft.Extensions.Logging;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class ClientConnectionManager : ObservableObject
    {
        [ObservableProperty]
        private ConnectionMethod _connectionMethod = ConnectionMethod.Auto;

        [ObservableProperty]
        private IWindowsManagementInstrumentationConnection _connection;

        [ObservableProperty]
        private IFileExplorer _fileExplorerConnection;

        private readonly ILogger<ClientConnectionManager> _logger;

        private readonly WindowsRemoteManagementClient _windowsRemoteManagementClient;
        private readonly WindowsManagementInstrumentationClient _windowsManagementInstrumentationClient;

        private readonly LocalFileExplorer _localClient;
        private readonly NetworkFileExplorer _networkClient;

        public ClientConnectionManager(ILogger<ClientConnectionManager> logger, WindowsRemoteManagementClient windowsRemoteManagementClient, WindowsManagementInstrumentationClient windowsManagementInstrumentationClient, LocalFileExplorer localClient, NetworkFileExplorer networkClient)
        {
            _logger = logger;
            _windowsRemoteManagementClient = windowsRemoteManagementClient;
            _windowsManagementInstrumentationClient = windowsManagementInstrumentationClient;
            _localClient = localClient;
            _networkClient = networkClient;

            FileExplorerConnection = _networkClient;

            Connection = windowsRemoteManagementClient;
        }

        public void SetConnectionMethod(ConnectionMethod connectionMethod)
        {
            if(ConnectionMethod == connectionMethod)
            {
                return;
            }

            Connection = connectionMethod switch
            {
                ConnectionMethod.Auto => _windowsRemoteManagementClient,
                ConnectionMethod.WMI => _windowsManagementInstrumentationClient,
                ConnectionMethod.WinRM => _windowsRemoteManagementClient,
                _ => throw new ArgumentException(null, nameof(connectionMethod)),
            };

            if(connectionMethod == ConnectionMethod.Auto)
            {
                try
                {
                    Connection.Connect("127.0.0.1");
                    FileExplorerConnection = _networkClient;
                }
                catch(Exception)
                {
                    Connection = _windowsManagementInstrumentationClient;
                    FileExplorerConnection = _localClient;
                }
            }

            ConnectionMethod = connectionMethod;

            _logger.LogInformation("Connection method changed to {method}", ConnectionMethod);
        }
    }
}
