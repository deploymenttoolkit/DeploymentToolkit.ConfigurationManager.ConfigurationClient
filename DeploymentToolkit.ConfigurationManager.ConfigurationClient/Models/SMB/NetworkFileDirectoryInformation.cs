using CommunityToolkit.Mvvm.ComponentModel;
using SMBLibrary;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB
{
    public partial class NetworkSMBFileDirectoryInformation : ObservableObject, IFileDirectoryInformation
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

        private readonly FileDirectoryInformation _instance;

        public NetworkSMBFileDirectoryInformation(FileDirectoryInformation instance, string path)
        {
            _instance = instance;
            Path = path;

            Name = instance.FileName;
            CreationTime = instance.CreationTime;
            LastAccessTime = instance.LastAccessTime;
            LastWriteTime = instance.LastWriteTime;
            ChangeTime = instance.ChangeTime;
        }

        public string GetFullPath() => System.IO.Path.Combine(Path, _instance.FileName);
    }
}
