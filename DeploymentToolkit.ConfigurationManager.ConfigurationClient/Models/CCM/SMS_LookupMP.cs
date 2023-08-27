using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class SMS_LookupMP : ObservableObject, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(SMS_LookupMP);
        public string Key => @$"Name=""{Name}""";

        [ObservableProperty]
        private string _capabilities;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _protocol;
        [ObservableProperty]
        private uint _version;
    }
}
