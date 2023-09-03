using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftMgmtAgent;
using System.Collections.Generic;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public interface IConfigurationManagerClientService
    {
        public IEnumerable<CCM_InstalledComponent>? GetInstalledComponents();

        public IEnumerable<CCM_ClientActions>? GetClientActions();

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
    }
}
