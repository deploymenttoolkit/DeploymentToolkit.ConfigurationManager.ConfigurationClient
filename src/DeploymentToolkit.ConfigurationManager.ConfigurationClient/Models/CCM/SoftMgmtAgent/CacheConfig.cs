using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftMgmtAgent
{
    public partial class CacheConfig : ObservableObject, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => CCM_Constants.ClientSoftMgmtAgentNamespace;
        public string Class => nameof(CacheConfig);
        public string Key => $@"ConfigKey=""{ConfigKey}""";
        public bool QueryByFilter => false;

        [ObservableProperty]
        private string _configKey;
        [ObservableProperty]
        private bool _inUse;
        [ObservableProperty]
        private string _location;
        [ObservableProperty]
        private uint _nextAvailableId;
        [ObservableProperty]
        private uint _size;
    }
}
