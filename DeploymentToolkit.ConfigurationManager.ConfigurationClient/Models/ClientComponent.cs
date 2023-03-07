using CommunityToolkit.Mvvm.ComponentModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class ClientComponent : ObservableObject
    {
        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string _state;
    }
}
