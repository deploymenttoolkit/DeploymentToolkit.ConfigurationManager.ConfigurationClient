using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;

public partial class WindowsFileExplorer : ObservableObject, IFileExplorer, IDisposable
{
    [ObservableProperty]
    private bool _isConnected;

    private string? _hostname;

    private readonly ILogger<WindowsFileExplorer> _logger;

    public WindowsFileExplorer(ILogger<WindowsFileExplorer> logger)
    {
        _logger = logger;
    }

    public bool Connect(string? hostname, string? username, string? password)
    {
        if(string.IsNullOrEmpty(hostname))
        {
            hostname = "127.0.0.1";
        }

        if(!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
        {
            throw new NotImplementedException();
        }

        _hostname = hostname;

        try
        {
            Directory.GetDirectories(TransformLocalPathToNetworkPath(@"C:\"));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {hostname}", hostname);
            return false;
        }

        IsConnected = true;

        return true;
    }

    public void Disconnect()
    {
        IsConnected = false;
    }

    public void Dispose()
    {
        Disconnect();
        GC.SuppressFinalize(this);
    }

    private string TransformLocalPathToNetworkPath(string path)
    {
        var driveLetter = path.Split(':')[0];
        var filePath = path.Substring(3, path.Length - 3);

        return $@"\\{_hostname}\{driveLetter}$\{filePath}";
    }

    public async Task<string> GetFileContent(string path)
    {
        var filePath = TransformLocalPathToNetworkPath(path);
        if(!File.Exists(filePath))
        {
            return string.Empty;
        }

        return File.ReadAllText(filePath);
    }

    public IEnumerable<IFileDirectoryInformation> GetFilesAndFolderInDirectory(string directory)
    {
        var path = TransformLocalPathToNetworkPath(directory);
        if(!Directory.Exists(path))
        {
            yield break;
        }

        _logger.LogTrace("Getting files from {path}", path);

        foreach(var filesAndFolders in Directory.GetDirectories(path).Union(Directory.GetFiles(path)))
        {
            yield return new LocalFileDirectoryInformation(filesAndFolders);
        }
    }

    public bool RemoveFile(string path)
    {
        var filePath = TransformLocalPathToNetworkPath(path);
        if (!File.Exists(filePath))
        {
            return true;
        }

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to remove file");
            return false;
        }
    }
}
