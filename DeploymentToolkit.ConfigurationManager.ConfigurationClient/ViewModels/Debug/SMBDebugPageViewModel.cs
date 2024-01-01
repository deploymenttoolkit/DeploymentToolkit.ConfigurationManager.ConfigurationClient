using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class SMBDebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _directoryPath;

        [ObservableProperty]
        private string _hostname;
        [ObservableProperty]
        private string _username = $"{Environment.GetEnvironmentVariable("Username")}@{Environment.GetEnvironmentVariable("USERDNSDOMAIN")}";
        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private int _selectedIndex;

        [ObservableProperty]
        private ObservableCollection<IFileDirectoryInformation> _files = new();

        [ObservableProperty]
        private string _fileContent;

        private readonly LocalFileExplorer _localClient;
        private readonly SMBClientFileExplorer _networkClient;

        public SMBDebugPageViewModel(LocalFileExplorer localClient, SMBClientFileExplorer networkClient)
        {
            _localClient = localClient;
            _networkClient = networkClient;
        }

        private IFileExplorer GetClient()
        {
            return SelectedIndex == 0 ? _localClient : _networkClient;
        }

        [RelayCommand]
        private void Connect()
        {
            try
            {
                GetClient().Connect(Hostname, Username, Password);
            }
            catch (NotImplementedException) { }
        }

        [RelayCommand]
        private void Disconnect()
        {
            GetClient().Disconnect();
        }

        [RelayCommand]
        private void ListFiles()
        {
            if(string.IsNullOrEmpty(DirectoryPath) || !GetClient().IsConnected)
            {
                return; 
            }

            Files.Clear();

            var files = GetClient().GetFilesAndFolderInDirectory(DirectoryPath);
            if(files != null)
            {
                foreach(var file in files)
                {
                    Files.Add(file);
                }
            }
        }

        [RelayCommand]
        private void GetContent()
        {
            if (string.IsNullOrEmpty(DirectoryPath) || !GetClient().IsConnected)
            {
                return;
            }

            var content = GetClient().GetFileContent(DirectoryPath).Result;
            FileContent = content;
        }
    }
}
