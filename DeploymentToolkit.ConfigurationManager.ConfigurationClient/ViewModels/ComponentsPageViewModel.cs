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

        private string GetStateNameFromState(CPAPPLETLib.EComponentState state)
        {
            switch (state)
            {
                case CPAPPLETLib.EComponentState.COMPONENT_STATE_DISABLED:
                    return "Disabled";

                case CPAPPLETLib.EComponentState.COMPONENT_STATE_ENABLED:
                    return "Enabled";

                case CPAPPLETLib.EComponentState.COMPONENT_STATE_INSTALLED:
                    return "Installed";
            }

            return "Unknown";
        }

        public ComponentsPageViewModel(ConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            foreach(CPAPPLETLib.ClientComponent component in _clientService.GetInstalledComponent())
            {
                Properties.Add(new ClientComponent()
                {
                    DisplayName = component.DisplayName,
                    Version = component.Version,
                    State = GetStateNameFromState(component.State)
                });
            }
        }
    }
}
