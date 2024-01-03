using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class CCM_Client : ObservableObject, IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(CCM_Client);

        [ObservableProperty]
        private string _clientId;
        [ObservableProperty]
        private string _clientIdChangeDate;
        [ObservableProperty]
        private string _previousClientId;
    }
}