using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class CCM_ClientUpgradeStatus : ObservableObject, IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(CCM_ClientUpgradeStatus);

        [ObservableProperty]
        private string _lastEnforcedBaselineCookie;
        [ObservableProperty]
        private string _lastUpgradeType;
        [ObservableProperty]
        private DateTime _lastUpgradeTime;
        [ObservableProperty]
        private string _baselineCookie;
        [ObservableProperty]
        private uint _duration = 0;
        [ObservableProperty]
        private uint _state;
        [ObservableProperty]
        private ulong _scheduledDownloadTime;
        [ObservableProperty]
        private uint _errorCode;
        [ObservableProperty]
        private uint _retryCount;
        [ObservableProperty]
        private string _lastReportBaseline;
        [ObservableProperty]
        private string _lastReportStateId = "0";
        [ObservableProperty]
        private string _reserved;
    }
}
