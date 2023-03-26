using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ConfigurationsViewModel : ObservableObject
    {
        private readonly ObservableCollection<DesiredStateConfiguration> _configurations = new();
        public ObservableCollection<DesiredStateConfiguration> Configurations => _configurations;

        private readonly ConfigurationManagerClientService _clientService;

        public ConfigurationsViewModel(ConfigurationManagerClientService clientService)
        {
            _clientService = clientService;
            
            foreach(var configuration in _clientService.GetDesiredStateConfiguration().Cast<ManagementObject>())
            {
                Configurations.Add(new DesiredStateConfiguration(this, configuration));
            }
        }

        [RelayCommand]
        private void Evaluate(string name)
        {
            try
            {
                var configuration = Configurations.FirstOrDefault(c => c.Name == name);
                if (configuration != null)
                {
                    configuration.Evaluate();
                    App.Current.DispatcherQueue.TryEnqueue(() =>
                    {
                        configuration.UpdateInstance();
                    });
                }
            }
            catch (Exception) { }
        }
    }
}
