using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vanara.Extensions;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;

public partial class SMBClientFileExplorer : ObservableObject, IFileExplorer, IDisposable
{
    [ObservableProperty]
    private bool _isConnected;

    private readonly ILogger<SMBClientFileExplorer> _logger;

    private SMB2Client? _client;
    private string? _hostname;

    public SMBClientFileExplorer(ILogger<SMBClientFileExplorer> logger)
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

        hostname ??= "127.0.0.1";

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
        else
        {
            username = string.Empty;
            password = string.Empty;
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
        _hostname = hostname;
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

    public async Task<string> GetFileContent(string path)
    {
        if(!IsConnected)
        {
            return string.Empty;
        }

        var driveLetter = path.Split(':')[0];
        var filePath = path.Substring(3, path.Length - 3);

        var fileStore = _client!.TreeConnect($"{driveLetter}$", out var shareStatus);
        if (shareStatus != NTStatus.STATUS_SUCCESS)
        {
            return string.Empty;
        }

        try
        {
            var file = fileStore.CreateFile(
                out var fileHandle,
                out var fileStatus,
                filePath,
                AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE,
                FileAttributes.Normal,
                ShareAccess.Read,
                CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                null
            );

            if (file != NTStatus.STATUS_SUCCESS)
            {
                return string.Empty;
            }

            try
            {
                var stream = new System.IO.MemoryStream();
                var bytesRead = 0L;
                while(true)
                {
                    var readStatus = fileStore.ReadFile(out var data, fileHandle, bytesRead, (int)_client.MaxReadSize);
                    if(readStatus != NTStatus.STATUS_SUCCESS && readStatus != NTStatus.STATUS_END_OF_FILE)
                    {
                        _logger.LogError("Failed to read file with status {status}", readStatus);
                        return string.Empty;
                    }

                    if (readStatus != NTStatus.STATUS_END_OF_FILE && data.Length > 0)
                    {
                        bytesRead += data.Length;
                        stream.Write(data, 0, data.Length);
                    }

                    if(readStatus == NTStatus.STATUS_END_OF_FILE)
                    {
                        await stream.FlushAsync();
                        stream.Position = 0;
                        var streamReader = new System.IO.StreamReader(stream, true);
                        return await streamReader.ReadToEndAsync();
                    }
                }
            }
            finally
            {
                fileStore.CloseFile(fileHandle);
            }
        }
        finally
        {
            fileStore.Disconnect();
        }
    }

    public IEnumerable<IFileDirectoryInformation> GetFilesAndFolderInDirectory(string directory)
    {
        if(!IsConnected)
        {
            yield break;
        }

        var driveLetter = directory.Split(':')[0];
        var path = directory.Substring(3, directory.Length - 3);
        var remotePath = $"\\\\{_hostname}\\{driveLetter}$\\{path}";

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

                    yield return new NetworkSMBFileDirectoryInformation(directoryOrFile, remotePath);
                }
            }
        }
        finally
        {
            fileStore.CloseFile(fileHandle);
            fileStore.Disconnect();
        }
    }

    public bool RemoveFile(string path)
    {
        if (!IsConnected)
        {
            return false;
        }

        var driveLetter = path.Split(':')[0];
        var filePath = path.Substring(3, path.Length - 3);

        var fileStore = _client!.TreeConnect($"{driveLetter}$", out var shareStatus);
        if (shareStatus != NTStatus.STATUS_SUCCESS)
        {
            return false;
        }

        try
        {
            var file = fileStore.CreateFile(
                out var fileHandle,
                out var fileStatus,
                filePath,
                AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE,
                FileAttributes.Normal,
                ShareAccess.None,
                CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                null
            );

            if (file != NTStatus.STATUS_SUCCESS)
            {
                return false;
            }

            try
            {
                var fileDispositionInformation = new FileDispositionInformation()
                {
                    DeletePending = true
                };
                var setFileStatus = fileStore.SetFileInformation(fileHandle, fileDispositionInformation);
                if(setFileStatus == NTStatus.STATUS_SUCCESS)
                {
                    return true;
                }

                return false;
            }
            finally
            {
                fileStore.CloseFile(fileHandle);
            }
        }
        finally
        {
            fileStore.Disconnect();
        }
    }
}
