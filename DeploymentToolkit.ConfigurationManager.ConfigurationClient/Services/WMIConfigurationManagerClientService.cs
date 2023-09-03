using CPAPPLETLib;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using System;
using System.Management;
using System.ServiceProcess;
using UIRESOURCELib;
using CacheElement = UIRESOURCELib.CacheElement;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class WMIConfigurationManagerClientService
    {
        private ManagementScope _clientManagementScope;

        private UACService _uacService;

        private UIResourceMgr _uiResourceMgr;
        //private SmsClient _smsClient;
        private CPAppletMgr _cpAppletManager;

        private ManagementScope _policyManagementScope = new ManagementScope(@"ROOT\ccm\Policy");

        public WMIConfigurationManagerClientService(UACService uacService)
        {
            _uacService = uacService;

            _clientManagementScope = new ManagementScope(@"ROOT\ccm");
            _clientManagementScope.Connect();

            _cpAppletManager = new CPAppletMgr();

            if (_uacService.IsElevated)
            {
                _uiResourceMgr = new UIResourceMgr();
                //_smsClient = new SmsClient();
            }
        }

        //internal List<PolicyRequestor> GetPolicyRequestors()
        //{
        //    if (!_uacService.IsElevated)
        //    {
        //        return null;
        //    }
        //    if (!_policyManagementScope.IsConnected)
        //    {
        //        _policyManagementScope.Connect();
        //    }

        //    var result = new List<PolicyRequestor>();
        //    var policyClass = new ManagementClass(_policyManagementScope, new ManagementPath("__namespace"), null);
        //    foreach (var subClass in policyClass.GetInstances())
        //    {
        //        result.Add(new PolicyRequestor(null, subClass as ManagementObject));
        //    }
        //    return result;
        //}

        public void RestartService()
        {
            if (!_uacService.IsElevated)
            {
                return;
            }

            var ccmExecService = new ServiceController("ccmexec");
            if(ccmExecService.Status == ServiceControllerStatus.Running)
            {
                ccmExecService.Stop();
                ccmExecService.WaitForStatus(ServiceControllerStatus.Stopped);
            }

            ccmExecService.Start();
        }

        public ClientComponents GetInstalledComponent()
        {
            return _cpAppletManager.GetClientComponents();
        }

        public ClientActions GetClientActions()
        {
            return _cpAppletManager.GetClientActions();
        }

        public CacheElements GetCache()
        {
            if (!_uacService.IsElevated)
            {
                return default;
            }
            return _uiResourceMgr.GetCacheInfo().GetCacheElements();
        }

        public uint GetCacheSize()
        {
            var cacheConfig = GetInstance(@"CacheConfig.ConfigKey=""Cache""", new ManagementScope(@"ROOT\ccm\SoftMgmtAgent"));
            return (uint)cacheConfig.GetPropertyValue("Size");
        }

        public void SetCacheSize(uint size)
        {
            var cacheConfig = GetInstance(@"CacheConfig.ConfigKey=""Cache""", new ManagementScope(@"ROOT\ccm\SoftMgmtAgent"));
            cacheConfig.SetPropertyValue("Size", size);
            cacheConfig.Put();
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

        public ManagementObjectCollection GetDesiredStateConfiguration()
        {
            return GetInstances("SMS_DesiredConfiguration", new ManagementScope(@"ROOT\ccm\dcm"));
        }

        public ManagementObjectCollection GetApplications()
        {
            return GetInstances("CCM_Application", new ManagementScope(@"ROOT\ccm\ClientSDK"));
        }

        public ManagementObject GetApplication(string id, string revision, bool isMachineTarget)
        {
            return GetInstance(@$"CCM_Application.Id=""{id}"",Revision=""{revision}"",IsMachineTarget={(isMachineTarget ? "true" : "false")}", new ManagementScope(@"ROOT\ccm\ClientSDK"));
        }

        //public uint InstallApplication(Application application, Priority priority, bool reboot = false) => InvokeApplicationMethod("Install", application, priority, reboot);
        //public uint RepairApplication(Application application, Priority priority, bool reboot = false) => InvokeApplicationMethod("Repair", application, priority, reboot);
        //public uint UninstallApplication(Application application, Priority priority, bool reboot = false) => InvokeApplicationMethod("Uninstall", application, priority, reboot);

        //private uint InvokeApplicationMethod(string method, Application application, Priority priority, bool reboot)
        //{
        //    var applicationClass = new ManagementClass(@"ROOT\ccm\ClientSDK", "CCM_Application", null);

        //    var parameters = applicationClass.GetMethodParameters(method);
        //    parameters["Id"] = application.Id;
        //    parameters["Revision"] = application.Revision;
        //    parameters["IsMachineTarget"] = application.IsMachineTarget;
        //    parameters["EnforcePreference"] = (uint)application.EnforcePreference;
        //    parameters["Priority"] = priority.ToString();
        //    parameters["IsRebootIfNeeded"] = reboot;

        //    var result = applicationClass.InvokeMethod(method, parameters, null);
        //    return Convert.ToUInt32(result["ReturnValue"]);
        //}

        public ManagementObjectCollection GetPrograms()
        {
            return GetInstances("CCM_Program", new ManagementScope(@"ROOT\ccm\ClientSDK"));
        }

        public ManagementObjectCollection GetSoftwareUpdates()
        {
            return GetInstances("CCM_SoftwareUpdate", new ManagementScope(@"ROOT\ccm\ClientSDK"));
        }

        public ManagementObject GetSoftwareUpdate(string id)
        {
            return GetInstance(@$"CCM_SoftwareUpdate.UpdateID=""{id}""", new ManagementScope(@"ROOT\ccm\ClientSDK"));
        }

        private ManagementObjectCollection GetInstances(string className, ManagementScope scope = default)
        {
            var selectedScope = scope == default ? _clientManagementScope : scope;
            if (!selectedScope.IsConnected)
            {
                selectedScope.Connect();
            }

            var searcher = new ManagementObjectSearcher(selectedScope, new ObjectQuery($"SELECT * FROM {className}"));
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

        private ManagementObject GetInstance(string path, ManagementScope scope = default)
        {
            var selectedScope = scope == default ? _clientManagementScope : scope;
            if(!selectedScope.IsConnected)
            {
                selectedScope.Connect();
            }

            var managementPath = new ManagementPath(selectedScope.Path + ":" + path);
            var managementObject = new ManagementObject()
            {
                Path = managementPath
            };
            managementObject.Get();
            return managementObject;
        }
    }
}
