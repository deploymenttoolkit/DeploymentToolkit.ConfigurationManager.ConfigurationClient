using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System.Collections.ObjectModel;
using System.Management;
using Vanara.PInvoke;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public partial class Policy : ObservableObject, IPolicy
    {
        public PolicyPageViewModel ViewModel { get; set; }

        public ObservableCollection<IPolicy> Children { get; set; } = new();

        public ObservableCollection<PolicyClass> Classes { get; set; } = new();

        [ObservableProperty]
        private string _displayName;

        private readonly ManagementObject _parent;

        public Policy(PolicyPageViewModel viewModel, ManagementObject parent, string policyName)
        {
            ViewModel = viewModel;
            _parent = parent;
            DisplayName = policyName;
        }

        public void UpdateInstances()
        {
            Classes.Clear();

            var namespacePath = $@"{_parent.Scope.Path.Path}\{_parent.GetPropertyValue("Name")}\{DisplayName}";
            var scope = new ManagementScope(namespacePath);
            var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM meta_class"));

            foreach (var subClass in searcher.Get())
            {
                // Skip system classes
                if (subClass.ClassPath.Path.Contains("__") || subClass.ClassPath.Path.Contains("CIM") || subClass.ClassPath.Path.Contains("MSFT"))
                {
                    continue;
                }

                Classes.Add(new PolicyClass(ViewModel, subClass as ManagementObject));
            }
        }
    }
}
