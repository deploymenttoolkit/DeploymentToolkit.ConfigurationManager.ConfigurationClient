using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using System.Collections.Generic;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public interface IFileExplorer
    {
        public bool IsConnected { get; set; }

        public bool Connect(string? hostname, string? username, string? password);
        public void Disconnect();

        public IEnumerable<IFileDirectoryInformation> GetFilesAndFolderInDirectory(string directory);
    }
}
