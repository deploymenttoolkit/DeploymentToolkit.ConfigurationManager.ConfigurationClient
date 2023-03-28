using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System.Collections.ObjectModel;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public partial class PolicyClass : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        public PolicyPageViewModel ViewModel { get; set; }

        public ObservableCollection<PolicyInstance> Instances { get; set; } = new();

        private readonly ManagementObject _policyClass;

        public PolicyClass(PolicyPageViewModel viewModel, ManagementObject policyClass)
        {
            ViewModel = viewModel;
            _policyClass = policyClass;

            Name = (string)policyClass.GetPropertyValue("__CLASS");
        }

        public void UpdateInstances()
        {
            Instances.Clear();

            var namespacePath = $@"{_policyClass.Scope.Path.Path}";
            var scope = new ManagementScope(namespacePath);
            var searcher = new ManagementObjectSearcher(scope, new ObjectQuery($"SELECT * FROM {Name}"));
            foreach (var instance in searcher.Get())
            {
                Instances.Add(new PolicyInstance(ViewModel, instance as ManagementObject));
            }
        }
    }
}
