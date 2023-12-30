using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class LocalFileExplorer : ObservableObject, IFileExplorer, IDisposable
    {
        [ObservableProperty]
        private bool _isConnected;

        private readonly ILogger<LocalFileExplorer> _logger;

        public LocalFileExplorer(ILogger<LocalFileExplorer> logger)
        {
            _logger = logger;
        }

        public bool Connect(string? hostname, string? username, string? password)
        {
            if(!string.IsNullOrEmpty(hostname) || !string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
            {
                throw new NotImplementedException();
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

        public async Task<string> GetFileContent(string path)
        {
            var filePath = Path.GetFullPath(path);
            if(!File.Exists(filePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(filePath);
        }

        public IEnumerable<IFileDirectoryInformation> GetFilesAndFolderInDirectory(string directory)
        {
            var path = Path.GetFullPath(directory);
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
    }
}
