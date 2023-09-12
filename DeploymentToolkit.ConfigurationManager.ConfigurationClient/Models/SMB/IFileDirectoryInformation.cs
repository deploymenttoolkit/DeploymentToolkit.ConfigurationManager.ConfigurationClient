using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB
{
    public interface IFileDirectoryInformation
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime ChangeTime { get; set; }

        public string GetFullPath();
    }
}
