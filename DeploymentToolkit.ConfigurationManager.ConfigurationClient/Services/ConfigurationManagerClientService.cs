using System.Management;
using UIRESOURCELib;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class ConfigurationManagerClientService
    {
        private ManagementScope _clientManagementScope;

        private UACService _uacService;

        private UIResourceMgr _uiResourceMgr;

        public ConfigurationManagerClientService(UACService uacService)
        {
            _uacService = uacService;

            _clientManagementScope = new ManagementScope(@"ROOT\ccm");
            _clientManagementScope.Connect();

            if(_uacService.IsElevated)
            {
                _uiResourceMgr = new UIResourceMgr();
            }
        }

        public CacheElements GetCache()
        {
            if (!_uacService.IsElevated)
            {
                return default;
            }
            return _uiResourceMgr.GetCacheInfo().GetCacheElements();
        }

        public void ClearCache(bool includePersistent)
        {
            if(!_uacService.IsElevated)
            {
                return;
            }

            var cacheInfo = _uiResourceMgr.GetCacheInfo();
            foreach (CacheElement element in cacheInfo.GetCacheElements())
            {
                cacheInfo.DeleteCacheElementEx(element.CacheElementId, includePersistent ? 1 : 0);
            }
        }

        public bool DeleteFromCache(string cacheElementId)
        {
            if (!_uacService.IsElevated)
            {
                return false;
            }

            var cacheInfo = _uiResourceMgr.GetCacheInfo();
            var cache = cacheInfo.GetCacheElements();
            foreach(CacheElement element in cache)
            {
                if(element.CacheElementId == cacheElementId)
                {
                    cacheInfo.DeleteCacheElementEx(element.CacheElementId, 1);
                    return true;
                }
            }
            return false;
        }

        public ManagementObject GetClient()
        {
            return GetInstance("CCM_Client=@");
        }

        public ManagementObject GetSMSClient()
        {
            return GetInstance("SMS_Client=@");
        }
        
        public ManagementBaseObject GetSMSLookupMP()
        {
            return GetFirstInstance("SMS_LookupMP");
        }

        public ManagementObject GetClientIdentificationInformation()
        {
            return GetInstance("CCM_ClientIdentificationInformation=@");
        }

        public ManagementObject GetClientSiteMode()
        {
            return GetInstance("CCM_ClientSiteMode=@");
        }

        public ManagementObject GetClientUpgradeStatus()
        {
            return GetInstance("CCM_ClientUpgradeStatus=@");
        }

        public ManagementObject GetClientInfo()
        {
            return GetInstance("ClientInfo=@");
        }

        public ManagementObjectCollection GetInstalledComponent()
        {
            return GetInstances("CCM_InstalledComponent");
        }

        private ManagementObjectCollection GetInstances(string className)
        {
            var searcher = new ManagementObjectSearcher(_clientManagementScope, new ObjectQuery($"SELECT * FROM {className}"));
            return searcher.Get();
        }

        private ManagementBaseObject GetFirstInstance(string className)
        {
            var searcher = new ManagementObjectSearcher(_clientManagementScope, new ObjectQuery($"SELECT * FROM {className}"));
            var results = searcher.Get();
            foreach (var result in results)
            {
                return result;
            }
            return default;
        }

        private ManagementObject GetInstance(string path)
        {
            var managementPath = new ManagementPath(_clientManagementScope.Path + ":" + path);
            var managementObject = new ManagementObject()
            {
                Path = managementPath
            };
            managementObject.Get();
            return managementObject;
        }
    }
}
