using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System.Collections.ObjectModel;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public partial class PolicyInstance : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        public PolicyPageViewModel ViewModel { get; set; }

        public ObservableCollection<Property> Properties { get; set; } = new();

        private ManagementObject _policyInstance;

        public PolicyInstance(PolicyPageViewModel viewModel, ManagementObject policyInstance)
        {
            ViewModel = viewModel;
            _policyInstance = policyInstance;

            Name = (string)policyInstance.GetPropertyValue("__Relpath");
        }

        public void UpdateProperties()
        {
            Properties.Clear();

            foreach (var property in _policyInstance.Properties)
            {
                Properties.Add(new Property(property.Name, property.Value));
            }
        }
    }
}
