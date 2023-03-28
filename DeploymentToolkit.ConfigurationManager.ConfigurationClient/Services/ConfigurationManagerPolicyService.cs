using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy;
using System.Collections.Generic;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class ConfigurationManagerClientService
    {
        private ManagementScope _policyManagementScope = new ManagementScope(@"ROOT\ccm\Policy");

        internal List<PolicyRequestor> GetPolicyRequestors()
        {
            if (!_uacService.IsElevated)
            {
                return null;
            }
            if (!_policyManagementScope.IsConnected)
            {
                _policyManagementScope.Connect();
            }

            var result = new List<PolicyRequestor>();
            var policyClass = new ManagementClass(_policyManagementScope, new ManagementPath("__namespace"), null);
            foreach (var subClass in policyClass.GetInstances())
            {
                result.Add(new PolicyRequestor(null, subClass as ManagementObject));
            }
            return result;
        }
    }
}
