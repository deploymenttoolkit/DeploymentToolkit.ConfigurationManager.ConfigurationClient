using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ApplicationsPageViewModel : ObservableObject
    {
        private ObservableCollection<Application> _applications = new();
        public ObservableCollection<Application> Applications => _applications;

        private ConfigurationManagerClientService _clientService;

        public ApplicationsPageViewModel(ConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            UpdateApplications();
        }

        [RelayCommand]
        private void UpdateApplications()
        {
            var applications = new List<Application>();
            foreach(ManagementObject application in _clientService.GetApplications())
            {
                applications.Add(new Application(this, application));
            }

            foreach(var application in applications.OrderBy(a => a.Name))
            {
                Applications.Add(application);
            }
        }
    }
}
