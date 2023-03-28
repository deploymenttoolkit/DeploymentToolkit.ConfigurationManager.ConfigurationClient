using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public partial class PolicyRequestor : ObservableObject, IPolicy
    {
        public PolicyPageViewModel ViewModel { get; set; }

        public ObservableCollection<IPolicy> Children { get; set; } = new();

        [ObservableProperty]
        private string _displayName;

        private readonly ManagementObject _policy;

        public PolicyRequestor(PolicyPageViewModel viewModel, ManagementObject policy)
        {
            ViewModel = viewModel;
            _policy = policy;

            Children.Add(new Policy(viewModel, policy, "ActualConfig"));
            Children.Add(new Policy(viewModel, policy, "RequestedConfig"));

            DisplayName = (string)policy.GetPropertyValue("Name");
        }
    }
}
