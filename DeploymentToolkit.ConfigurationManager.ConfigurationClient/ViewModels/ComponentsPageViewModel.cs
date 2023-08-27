using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public class ComponentsPageViewModel : ObservableObject
    {
        public ObservableCollection<CCM_InstalledComponent> Components { get; } = new();

        private readonly IConfigurationManagerClientService _clientService;

        public ComponentsPageViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            var components = _clientService.GetInstalledComponents();
            if (components != null)
            {
                foreach (var component in components)
                {
                    Components.Add(component);
                }
            }
        }
    }
}
