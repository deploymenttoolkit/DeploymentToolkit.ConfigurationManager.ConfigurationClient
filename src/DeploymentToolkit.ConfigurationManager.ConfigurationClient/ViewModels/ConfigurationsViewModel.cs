using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.dcm;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ConfigurationsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<SMS_DesiredConfiguration> _configurations = new();

        private readonly IConfigurationManagerClientService _clientService;
         
        public ConfigurationsViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;
            
            foreach(var configuration in _clientService.GetDesiredStateConfiguration())
            {
                configuration.ViewModel = this;
                Configurations.Add(configuration);
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
                    _clientService.EvaluateDesiredStateConfiguration(configuration);
                    App.Current.DispatcherQueue.TryEnqueue(() =>
                    {
                        _clientService.UpdateInstance<SMS_DesiredConfiguration>(configuration);
                    });
                }
            }
            catch (Exception) { }
        }
    }
}
