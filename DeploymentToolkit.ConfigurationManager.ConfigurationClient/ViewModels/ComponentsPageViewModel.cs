using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public class ComponentsPageViewModel : ObservableObject
    {
        private ObservableCollection<ClientComponent> _properties = new();
        public ObservableCollection<ClientComponent> Properties => _properties;

        private readonly ConfigurationManagerClientService _clientService;

        public ComponentsPageViewModel(ConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            foreach(var component in _clientService.GetInstalledComponent())
            {
                Properties.Add(new ClientComponent()
                {
                    DisplayName = component.Properties["DisplayName"].Value.ToString(),
                    Version = component.Properties["Version"].Value.ToString(),
                });
            }
        }
    }
}
