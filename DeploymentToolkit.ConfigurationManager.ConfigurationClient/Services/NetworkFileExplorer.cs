using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanara.Extensions;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class NetworkFileExplorer : ObservableObject, FileExplorer, IDisposable
    {
        [ObservableProperty]
        private bool _isConnected;

        private readonly ILogger<NetworkFileExplorer> _logger;

        private SMB2Client? _client;

        public NetworkFileExplorer(ILogger<NetworkFileExplorer> logger)
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
            if(!_client.Connect(hostname, SMBTransportType.DirectTCPTransport))
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

            if(status != NTStatus.STATUS_SUCCESS)
            {
                _logger.LogDebug("Failed to connected to {host}: {status}", hostname, status);
                return false;
            }

            IsConnected = true;

            _logger.LogDebug("Successfully connected to {host}", hostname);
            return true;
        }

        public void Disconnect()
        {
            if (_client?.IsConnected ?? false)
            {
                _client.Disconnect();
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public IEnumerable<IFileDirectoryInformation> GetFilesAndFolderInDirectory(string directory)
        {
            if(!IsConnected)
            {
                yield break;
            }

            var driveLetter = directory.Split(':')[0];
            var path = directory.Substring(3, directory.Length - 3);

            var fileStore = _client!.TreeConnect($"{driveLetter}$", out var shareStatus);
            if(shareStatus != NTStatus.STATUS_SUCCESS)
            {
                yield break;
            }

            var fileNTStatus = fileStore.CreateFile(
                out var fileHandle,
                out _,
                path,
                AccessMask.GENERIC_READ,
                FileAttributes.Directory,
                ShareAccess.Read | ShareAccess.Write,
                CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_DIRECTORY_FILE,
                null
            );

            if(fileNTStatus != NTStatus.STATUS_SUCCESS)
            {
                yield break;
            }

            try
            {
                var directoryStatus = fileStore.QueryDirectory(out var fileList, fileHandle, "*", FileInformationClass.FileDirectoryInformation);
                if (directoryStatus == NTStatus.STATUS_NO_MORE_FILES)
                {
                    foreach(var directoryOrFile in fileList.Cast<FileDirectoryInformation>())
                    {
                        _logger.LogTrace("{Type} - {Name}", directoryOrFile.FileAttributes.IsFlagSet(FileAttributes.Directory) ? "Dir" : "File", directoryOrFile.FileName);
                        if(directoryOrFile.FileName == "." || directoryOrFile.FileName == "..")
                        {
                            continue;
                        }

                        yield return new NetworkSMBFileDirectoryInformation(directoryOrFile, directory);
                    }
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
