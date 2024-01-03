using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftMgmtAgent
{
    public partial class CacheInfoEx : ObservableObject, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => CCM_Constants.ClientSoftMgmtAgentNamespace;
        public string Class => nameof(CacheInfoEx);
        public string Key => $@"CacheId=""{CacheId}""";
        public bool QueryByFilter => false;

        [ObservableProperty]
        private string _cacheId;
        [ObservableProperty]
        private string _contentId;
        [ObservableProperty]
        private string _contentVer;
        [ObservableProperty]
        private uint _referenceCount;
        [ObservableProperty]
        private string _location;
        [ObservableProperty]
        private uint _contentSize;
        [ObservableProperty]
        private DateTime _lastReferenced;
        [ObservableProperty]
        private string _contentType;
        [ObservableProperty]
        private bool _peerCaching;
        [ObservableProperty]
        private string _excludeFileList;
        [ObservableProperty]
        private uint _persistInCache;
        [ObservableProperty]
        private uint _contentFlags;
        [ObservableProperty]
        private ulong _deploymentFlags;
        [ObservableProperty]
        private string _partialContentInfo;
        [ObservableProperty]
        private bool _contentComplete = true;
        [ObservableProperty]
        private string _contentManifest = "";
    }
}