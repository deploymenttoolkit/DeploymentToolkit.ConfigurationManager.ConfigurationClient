using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class CCM_InstalledComponent : ObservableObject, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(CCM_InstalledComponent);
        public string Key => $@"Name=""{Name}""";
        public bool QueryByFilter => false;

        [ObservableProperty]
        private string _displayName;
        [ObservableProperty]
        private string _displayNameResourceFile;
        [ObservableProperty]
        private uint _displayNameResourceID;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _version;
    }
}
