using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CPAPPLETLib;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ActionsPageViewModel : ObservableObject
    {
        private ObservableCollection<ClientActionWrapper> _actions = new();
        public ObservableCollection<ClientActionWrapper> Actions => _actions;

        private ConfigurationManagerClientService _clientService;

        public ActionsPageViewModel(ConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            foreach (ClientAction action in _clientService.GetClientActions())
            {
                Actions.Add(new ClientActionWrapper(this, action));
            }
        }

        [RelayCommand]
        public void RunAction(string id)
        {
            var action = Actions.FirstOrDefault(a => a.ActionId == id);
            if(action == null)
            {
                return;
            }

            action.PerformAction();
        }
    }
}
