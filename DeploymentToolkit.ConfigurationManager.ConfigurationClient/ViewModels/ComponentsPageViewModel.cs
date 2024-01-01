using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public class ComponentsPageViewModel : ObservableObject
    {
        public ObservableCollection<CCM_InstalledComponent> Components { get; } = new();

        private readonly ILogger<ComponentsPageViewModel> _logger;
        private readonly IConfigurationManagerClientService _clientService;

        public ComponentsPageViewModel(ILogger<ComponentsPageViewModel> logger, IConfigurationManagerClientService clientService)
        {
            _logger = logger;
            _clientService = clientService;

            var components = _clientService.GetInstalledComponents();
            if (components != null)
            {
                foreach (var component in components.OrderBy(c => c.Name))
                {
                    Components.Add(component);
                }
            }

            Task.Factory.StartNew(() => UpdateComponentStatus());
        }

        private void UpdateComponentStatus()
        {
            var componentClientConfig = _clientService.GetComponentClientConfigs();
            if(componentClientConfig == null)
            {
                return;
            }

            foreach(var componentStatus in componentClientConfig)
            {
                var component = Components.FirstOrDefault(c => c.Name == componentStatus.ComponentName);
                if(component == null)
                {
                    _logger.LogInformation("Failed to find component status for {component}", componentStatus.ComponentName);
                    continue;
                }

                App.Current.DispatcherQueue.TryEnqueue(() =>
                {
                    if (componentStatus.Enabled)
                    {
                        component.Status = "Enabled";
                    }
                    else
                    {
                        component.Status = "Disabled";
                    }
                });
            }
        }
    }
}
