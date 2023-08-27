using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy
{
    public partial class PolicyClass : ObservableObject, IPolicy, IWindowsManagementInstrumentationInstance
    {
        public string Namespace
        {
            get
            {
                if (Default)
                {
                    return $@"{CCM_Constants.ClientPolicyNamespace}\Default{PolicyTarget}\{ConfigState}Config";
                }

                if (PolicyTarget == Policy.PolicyTarget.Machine)
                {
                    return $@"{CCM_Constants.ClientPolicyNamespace}\Machine\{ConfigState}Config";
                }

                return $@"{CCM_Constants.ClientPolicyNamespace}\{SID}\{ConfigState}Config";
            }
        }
        public string Class => DisplayName;
        public string Key => throw new NotImplementedException();

        [ObservableProperty]
        private string _displayName;

        public ObservableCollection<IPolicy> Children { get; set; } = new();

        public bool Default { get; }
        public PolicyTarget? PolicyTarget { get; }
        public ConfigState? ConfigState { get; }
        public string? SID { get; }

        public PolicyClass(string displayName,
            bool defaultPolicy,
            PolicyTarget? policyTarget,
            ConfigState? configState,
            string? sid = null)
        {
            DisplayName = displayName;

            Default = defaultPolicy;
            PolicyTarget = policyTarget;
            ConfigState = configState;
            SID = sid;
        }
    }
}
