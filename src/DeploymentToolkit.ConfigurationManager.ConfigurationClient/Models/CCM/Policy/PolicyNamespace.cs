using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy
{
    public partial class PolicyNamespace : ObservableObject, IPolicy, IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace
        {
            get
            {
                if (Default)
                {
                    if (ConfigState.HasValue)
                    {
                        return $@"{CCM_Constants.ClientPolicyNamespace}\Default{PolicyTarget}\{ConfigState}Config";
                    }
                    else if(PolicyTarget.HasValue)
                    {
                        return $@"{CCM_Constants.ClientPolicyNamespace}\Default{PolicyTarget}";
                    }
                }
                else
                {
                    if(PolicyTarget.HasValue)
                    {
                        if(ConfigState.HasValue)
                        {
                            if(PolicyTarget == Policy.PolicyTarget.Machine)
                            {
                                return $@"{CCM_Constants.ClientPolicyNamespace}\Machine\{ConfigState}Config";
                            }
                            return $@"{CCM_Constants.ClientPolicyNamespace}\{SID}\{ConfigState}Config";
                        }
                        else
                        {
                            if (PolicyTarget == Policy.PolicyTarget.Machine)
                            {
                                return $@"{CCM_Constants.ClientPolicyNamespace}\Machine";
                            }
                            return $@"{CCM_Constants.ClientPolicyNamespace}\{SID}";
                        }
                    }
                }

                return CCM_Constants.ClientPolicyNamespace;
            }
        }
        public string Class => DisplayName;

        [ObservableProperty]
        private string _displayName;

        public ObservableCollection<IPolicy> Children { get; set; } = new();

        public bool Default { get; }
        public PolicyTarget? PolicyTarget { get; }
        public ConfigState? ConfigState { get; }
        public string? SID { get; }

        public PolicyNamespace(
            string displayName,
            IEnumerable<IPolicy>? children,
            bool defaultPolicy = false,
            PolicyTarget? policyTarget = null,
            ConfigState? configState = null,
            string? sid = null
        )
        {
            DisplayName = displayName;

            Default = defaultPolicy;
            PolicyTarget = policyTarget;
            ConfigState = configState;
            SID = sid;

            if (children != null)
            {
                foreach (var child in children)
                {
                    Children.Add(child);
                }
            }
        }
    }
}
