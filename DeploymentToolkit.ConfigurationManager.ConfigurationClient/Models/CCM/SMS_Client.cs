using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class SMS_Client : ObservableObject, IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(SMS_Client);

        [ObservableProperty]
        private bool _allowLocalAdminOverride;
        [ObservableProperty]
        private uint _clientType;
        [ObservableProperty]
        private string _clientVersion;
        [ObservableProperty]
        private bool _enableAutoAssignment;
    }
}
