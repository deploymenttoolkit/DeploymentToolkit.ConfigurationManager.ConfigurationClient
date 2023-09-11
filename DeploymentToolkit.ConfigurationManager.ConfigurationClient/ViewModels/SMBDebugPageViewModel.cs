using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class SMBDebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _directoryPath;

        [ObservableProperty]
        private string _hostname;
        [ObservableProperty]
        private string _username;
        [ObservableProperty]
        private string _password;

        private readonly SMBClient _client;

        public SMBDebugPageViewModel(SMBClient client)
        {
            _client = client;
        }

        [RelayCommand]
        private void Connect()
        {
            if(string.IsNullOrEmpty(DirectoryPath))
            {
                return;
            }

            _client.Connect(Hostname, Username, Password);
        }
    }
}
