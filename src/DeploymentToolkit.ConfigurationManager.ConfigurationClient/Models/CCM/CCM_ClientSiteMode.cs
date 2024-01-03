using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class CCM_ClientSiteMode : ObservableObject, IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(CCM_ClientSiteMode);

        [ObservableProperty]
        private uint _certKeyType;
        [ObservableProperty]
        private uint _communicationMode;
        [ObservableProperty]
        private uint _siteMode;
    }
}
