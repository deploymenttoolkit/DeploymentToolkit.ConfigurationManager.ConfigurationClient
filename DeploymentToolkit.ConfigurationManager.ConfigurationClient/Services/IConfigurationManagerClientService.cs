using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.dcm;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftMgmtAgent;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public interface IConfigurationManagerClientService
    {
        public T UpdateInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : class, IWindowsManagementInstrumentationInstance, new();
        public T PutInstance<T>(IWindowsManagementInstrumentationInstance instance, params string[] updatedProperties) where T : class, IWindowsManagementInstrumentationInstance, new();

        public IEnumerable<CCM_InstalledComponent>? GetInstalledComponents();

        public IEnumerable<CCM_ClientAction>? GetClientActions();

        public uint PerformClientAction(CCM_ClientAction clientAction);

        public IEnumerable<CacheInfoEx>? GetClientCache();
        public CacheConfig? GetClientCacheConfig();

        public CCM_Client? GetClient();
        public SMS_Client? GetSMSClient();
        public SMS_LookupMP? GetSMSLookupMP();
        public CCM_ClientIdentificationInformation? GetClientIdentificationInformation();
        public CCM_ClientSiteMode? GetClientSiteMode();
        public CCM_ClientUpgradeStatus? GetClientUpgradeStatus();
        public ClientInfo? GetClientInfo();

        public IEnumerable<PolicyNamespace> GetPolicy();
        public IEnumerable<PolicyClass> GetPolicyClasses(PolicyNamespace policy);
        public IEnumerable<PolicyInstance> GetPolicyInstances(PolicyClass policyClass);

        public IEnumerable<CCM_Program> GetPrograms();

        public IEnumerable<CCM_Application> GetApplications();
        public uint InstallApplication(CCM_Application application, Priority priority, bool reboot = false);
        public uint RepairApplication(CCM_Application application, Priority priority, bool reboot = false);
        public uint UninstallApplication(CCM_Application application, Priority priority, bool reboot = false);

        public IEnumerable<CCM_SoftwareUpdate> GetSoftwareUpdates();

        public IEnumerable<SMS_DesiredConfiguration> GetDesiredStateConfiguration();
        public uint EvaluateDesiredStateConfiguration(SMS_DesiredConfiguration configuration);

        public Task RestartServiceAsync();
    }
}
