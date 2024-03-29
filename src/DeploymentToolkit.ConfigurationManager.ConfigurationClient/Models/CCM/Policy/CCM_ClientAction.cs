﻿using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy
{
    public partial class CCM_ClientAction : ObservableObject, IPolicy, IWindowsManagementInstrumentationInstance
    {
        public string Namespace
        {
            get
            {
                if(Default)
                {
                    return $@"{CCM_Constants.ClientPolicyNamespace}\Default{PolicyTarget}\{ConfigState}Config";
                }

                if(PolicyTarget == Policy.PolicyTarget.Machine)
                {
                    return $@"{CCM_Constants.ClientPolicyNamespace}\Machine\{ConfigState}Config";
                }

                return $@"{CCM_Constants.ClientPolicyNamespace}\{SID}\{ConfigState}Config";
            }
        }
        public string Class => "CCM_ClientActions";
        public string Key => $@"ActionID=""{ActionID}""";
        public bool QueryByFilter => false;

        [ObservableProperty]
        private string _displayName;

        public ObservableCollection<IPolicy> Children { get; set; } = new();

        public bool Default { get; }
        public PolicyTarget? PolicyTarget { get; }
        public ConfigState? ConfigState { get; }
        public string? SID { get; }
        public ActionsPageViewModel ViewModel { get; set; }

        public CCM_ClientAction()
        {

        }

        public CCM_ClientAction(bool defaultPolicy, PolicyTarget policyTarget, ConfigState configState, string? securityID = null)
        {
            Default = defaultPolicy;
            PolicyTarget = policyTarget;
            ConfigState = configState;
            SID = securityID;
        }

        [ObservableProperty]
        private string _actionID;
        [ObservableProperty]
        private string _displayNameResFilename;
        [ObservableProperty]
        private uint _displayNameResID;
        [ObservableProperty]
        private string _endpoint;
        [ObservableProperty]
        private string _message;
        [ObservableProperty]
        private string _name;
    }
}
