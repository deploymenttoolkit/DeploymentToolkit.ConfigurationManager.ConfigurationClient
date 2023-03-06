using CommunityToolkit.Mvvm.ComponentModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class ClientProperty : ObservableObject
    {
        public string Group { get; set; }

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _value;
    }
}
