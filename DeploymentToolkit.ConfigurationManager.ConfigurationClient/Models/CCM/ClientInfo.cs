using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class ClientInfo : ObservableObject, IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(ClientInfo);

        [ObservableProperty]
        private bool _inInternet;
        [ObservableProperty]
        private DateTime _internetModeLastUpdateTime;
        [ObservableProperty]
        private int _reserved1;
        [ObservableProperty]
        private string _reserved2;
    }
}
