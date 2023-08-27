using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class PolicyPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private Visibility _errorMessageVisibility = Visibility.Collapsed;
        [ObservableProperty]
        private Visibility _contentVisibility = Visibility.Visible;

        [ObservableProperty]
        private IPolicy _selectedItem;

        [ObservableProperty]
        private CollectionViewSource _classes = new();
        [ObservableProperty]
        private CollectionViewSource _instances = new();
        [ObservableProperty]
        private CollectionViewSource _properties = new();

        [ObservableProperty]
        private bool _classesBladeIsOpen = false;
        [ObservableProperty]
        private bool _instancesBladeIsOpen = false;
        [ObservableProperty]
        private bool _propertiesBladeIsOpen = false;

        private readonly UACService _uacService;
        private readonly IConfigurationManagerClientService _clientService;

        public ObservableCollection<PolicyNamespace> Policies = new();

        public PolicyPageViewModel(UACService uacService, IConfigurationManagerClientService clientService)
        {
            _uacService = uacService;
            _clientService = clientService;

            if(!_uacService.IsElevated)
            {
                _errorMessageVisibility = Visibility.Visible;
                _contentVisibility = Visibility.Collapsed;
                return;
            }

            var policies = _clientService.GetPolicy();
            foreach (var policy in policies.OrderBy(p => p.DisplayName))
            {
                Policies.Add(policy);
            }
        }

        [RelayCommand]
        private void RestartButton()
        {
            _uacService.RestartAsAdmin();
        }

        [RelayCommand]
        private void Expanding(TreeViewExpandingEventArgs args)
        {
            //(args.Item as PolicyBase)?.UpdateInstances();
        }

        [RelayCommand]
        private void Collapsed(TreeViewCollapsedEventArgs args)
        {
            ClassesBladeIsOpen = false;
            InstancesBladeIsOpen = false;
            PropertiesBladeIsOpen = false;
        }

        [RelayCommand]
        private void Selected(TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is not PolicyNamespace policy)
            {
                return;
            }

            Classes.Source = _clientService.GetPolicyClasses(policy).OrderBy(c => c.DisplayName);
            Instances.Source = null;
            Properties.Source = null;
            ClassesBladeIsOpen = true;
            InstancesBladeIsOpen = false;
            PropertiesBladeIsOpen = false;
        }

        [RelayCommand]
        private void PolicyClassSelected(ItemClickEventArgs args)
        {
            if (args.ClickedItem is not PolicyClass policyClass)
            {
                return;
            }

            Instances.Source = _clientService.GetPolicyInstances(policyClass).OrderBy(c => c.DisplayName);
            Properties.Source = null;
            InstancesBladeIsOpen = true;
            PropertiesBladeIsOpen = false;
        }

        [RelayCommand]
        private void PolicyInstanceSelected(ItemClickEventArgs args)
        {
            if (args.ClickedItem is not PolicyInstance instance)
            {
                return;
            }

            Properties.Source = instance.Properties.OrderBy(p => p.Name);
            PropertiesBladeIsOpen = true;
        }
    }
}
