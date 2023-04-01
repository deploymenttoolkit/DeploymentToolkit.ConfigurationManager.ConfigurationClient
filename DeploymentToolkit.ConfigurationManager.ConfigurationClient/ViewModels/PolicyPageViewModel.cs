using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy;
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

        private UACService _uacService;
        private ConfigurationManagerClientService _clientService;

        public ObservableCollection<PolicyRequestor> Policies = new();

        public PolicyPageViewModel(UACService uacService, ConfigurationManagerClientService clientService)
        {
            _uacService = uacService;
            _clientService = clientService;

            if(!_uacService.IsElevated)
            {
                _errorMessageVisibility = Visibility.Visible;
                _contentVisibility = Visibility.Collapsed;
                return;
            }

            foreach(var policy in _clientService.GetPolicyRequestors().OrderBy(p => p.DisplayName))
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
            if(args.InvokedItem is not Policy policy)
            {
                return;
            }

            policy.UpdateInstances();
            Classes.Source = policy.Classes.OrderBy(c => c.Name);
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

            policyClass.UpdateInstances();
            Instances.Source = policyClass.Instances.OrderBy(i => i.Name);
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

            instance.UpdateProperties();
            Properties.Source = instance.Properties.OrderBy(p => p.Name);
            PropertiesBladeIsOpen = true;
        }
    }
}
