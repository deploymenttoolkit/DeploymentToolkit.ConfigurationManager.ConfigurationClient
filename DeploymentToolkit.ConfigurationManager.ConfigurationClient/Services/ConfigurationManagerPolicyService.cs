using System;
using System.Diagnostics;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class ConfigurationManagerClientService
    {
        private ManagementScope _policyManagementScope = new ManagementScope(@"ROOT\ccm\Policy");

        internal void GetPolicies()
        {
            if(!_policyManagementScope.IsConnected)
            {
                _policyManagementScope.Connect();
            }

            var policyClass = new ManagementClass(_policyManagementScope, new ManagementPath("__namespace"), null);
            foreach (var subClass in policyClass.GetInstances())
            {
                Debug.WriteLine($"{subClass.Properties["Name"].Value}");
            }
        }
    }
}
