using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.cimv2
{
    public partial class Win32_Service : Win32_BaseService, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => @"ROOT\cimv2";
        public string Class => nameof(Win32_Service);
        public string Key => $@"Name=""{Name}""";
        public bool QueryByFilter => false;

        [ObservableProperty]
        private uint _checkPoint;
        [ObservableProperty]
        private uint _waitHint;
        [ObservableProperty]
        private uint _processId;
        [ObservableProperty]
        private bool _delayedAutoStart;
    }
}
