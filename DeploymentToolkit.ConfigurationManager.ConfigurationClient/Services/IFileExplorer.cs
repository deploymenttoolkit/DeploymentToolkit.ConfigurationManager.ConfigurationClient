using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public interface IFileExplorer
    {
        public bool IsConnected { get; set; }

        public bool Connect(string? hostname, string? username, string? password);
        public void Disconnect();

        public IEnumerable<IFileDirectoryInformation> GetFilesAndFolderInDirectory(string directory);
        public Task<string> GetFileContent(string path);
        public bool RemoveFile(string path);
    }
}
