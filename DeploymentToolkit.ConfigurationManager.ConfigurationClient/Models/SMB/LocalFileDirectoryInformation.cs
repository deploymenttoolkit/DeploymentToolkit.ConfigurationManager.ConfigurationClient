using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB
{
    public partial class LocalFileDirectoryInformation : ObservableObject, IFileDirectoryInformation
    {
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _path;

        [ObservableProperty]
        private DateTime _creationTime;
        [ObservableProperty]
        private DateTime _lastAccessTime;
        [ObservableProperty]
        private DateTime _lastWriteTime;
        [ObservableProperty]
        private DateTime _changeTime;

        public object ViewModel { get; set; }

        private string _fullPath;

        public LocalFileDirectoryInformation(string path)
        {
            _fullPath = path;

            Name = System.IO.Path.GetFileName(path);
            Path = Directory.GetParent(path)?.FullName ?? string.Empty;

            var attributes = File.GetAttributes(path);
            if(attributes.HasFlag(FileAttributes.Directory))
            {
                var directoryInfo = new DirectoryInfo(path);
                CreationTime = directoryInfo.CreationTime;
                LastAccessTime = directoryInfo.LastAccessTime;
                LastWriteTime = directoryInfo.LastWriteTime;
                ChangeTime = directoryInfo.LastWriteTime;
            }
            else
            {
                var fileInfo = new FileInfo(path);
                CreationTime = fileInfo.CreationTime;
                LastAccessTime = fileInfo.LastAccessTime;
                LastWriteTime = fileInfo.LastWriteTime;
                ChangeTime = fileInfo.LastWriteTime;
            }
        }

        public string GetFullPath() => _fullPath;
    }
}
