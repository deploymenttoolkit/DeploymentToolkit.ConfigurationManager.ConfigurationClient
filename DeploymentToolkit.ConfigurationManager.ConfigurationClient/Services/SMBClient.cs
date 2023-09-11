using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using SMBLibrary.Client;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class SMBClient : ObservableObject, INetworkFileExplorer, IDisposable
    {
        [ObservableProperty]
        private bool _isConnected;

        private readonly ILogger<SMBClient> _logger;

        private SMB2Client? _client;

        public SMBClient(ILogger<SMBClient> logger)
        {
            _logger = logger;
        }

        public bool Connect(string? hostname, string? username, string? password)
        {
            if(_client != null && _client.IsConnected)
            {
                _client.Disconnect();
            }

            IsConnected = false;

            _logger.LogDebug("Trying to connect to {host} as {user}", hostname, username);
            _client = new SMB2Client();
            if(!_client.Connect(hostname, SMBLibrary.SMBTransportType.DirectTCPTransport))
            {
                return false;
            }

            var domain = string.Empty;
            if (username != null)
            {
                if (username.Contains('@'))
                {
                    var split = username.Split('@');
                    domain = split[1];
                    username = split[0];
                }
                else if (username.Contains('\\'))
                {
                    var split = username.Split('\\');
                    domain = split[0];
                    username = split[1];
                }
            }

            _logger.LogTrace("Domain: {domain}, User: {user}", domain, username);
            var status = _client.Login(domain, username, password);

            if(status != SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                _logger.LogDebug("Failed to connected to {host}: {status}", hostname, status);
                return false;
            }

            IsConnected = true;

            _logger.LogDebug("Successfully connected to {host}", hostname);
            return true;
        }

        public void Dispose()
        {
            if(_client?.IsConnected ?? false)
            {
                _client.Disconnect();
            }
        }

        public void GetFilesInDirectory(string directory)
        {
            if(IsConnected)
            {
                return;
            }

            var driveLetter = directory.Split(':')[0];

            var fileStore = _client!.TreeConnect($"{driveLetter}$", out var shareStatus);
            if(shareStatus != SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                return;
            }

            var fileNTStatus = fileStore.CreateFile(
                out var fileHandle,
                out _,
                "\\",
                SMBLibrary.AccessMask.GENERIC_READ,
                SMBLibrary.FileAttributes.Directory,
                SMBLibrary.ShareAccess.Read,
                SMBLibrary.CreateDisposition.FILE_OPEN,
                SMBLibrary.CreateOptions.FILE_DIRECTORY_FILE,
                null
            );

            if(fileNTStatus != SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                return;
            }

            try
            {
                var directoryStatus = fileStore.QueryDirectory(out var fileList, fileHandle, "\\*", SMBLibrary.FileInformationClass.FileBasicInformation);
                if (directoryStatus != SMBLibrary.NTStatus.STATUS_SUCCESS)
                {
                    return;
                }
            }
            finally
            {
                fileStore.CloseFile(fileHandle);
                fileStore.Disconnect();
            }
        }
    }
}
