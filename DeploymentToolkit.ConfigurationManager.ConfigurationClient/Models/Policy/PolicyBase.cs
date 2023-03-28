using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public partial class PolicyBase : ObservableObject
    {
        public PolicyPageViewModel ViewModel { get; set; }

        public ObservableCollection<PolicyBase> Children { get; set; } = new();

        public ObservableCollection<PolicyBase> Instances { get; set; } = new();

        [ObservableProperty]
        private string _displayName;

        private ManagementObject _policy;

        public PolicyBase(PolicyPageViewModel viewModel, ManagementObject policy)
        {
            ViewModel = viewModel;
            _policy = policy;

            Debug.WriteLine($"{policy}");
            foreach(var property in policy.Properties)
            {
                Debug.WriteLine($"{property.Name} = {property.Value}");
            }
            foreach (var property in policy.SystemProperties)
            {
                Debug.WriteLine($"{property.Name} = {property.Value}");
            }

            var nameProperty = string.Empty;
            try
            {
                policy.GetPropertyValue("Name");
                nameProperty = "Name";
            }
            catch(ManagementException)
            {
                nameProperty = "__CLASS";
            }

            DisplayName = (string)policy.GetPropertyValue(nameProperty);

            if(string.IsNullOrEmpty(DisplayName))
            {
                return;
            }

            var newScope = new ManagementScope($@"{policy.Scope.Path.Path}\{policy.GetPropertyValue(nameProperty)}");

            try
            {
                var policyClass = new ManagementClass(newScope, new ManagementPath("__namespace"), null);
                foreach (var subNamespace in policyClass.GetInstances())
                {
                    Children.Add(new PolicyBase(viewModel, subNamespace as ManagementObject));
                }
            }
            catch (ManagementException) { }
        }

        public void UpdateInstances()
        {
            var searcher = new ManagementObjectSearcher(_policy.Scope, new ObjectQuery($"SELECT * FROM meta_class"));
            foreach (var subClass in searcher.Get())
            {
                // Skip system classes
                if (subClass.ClassPath.Path.Contains("__") || subClass.ClassPath.Path.Contains("CIM") || subClass.ClassPath.Path.Contains("MSFT"))
                {
                    continue;
                }
                Instances.Add(new PolicyBase(ViewModel, subClass as ManagementObject));
            }
        }
    }
}
