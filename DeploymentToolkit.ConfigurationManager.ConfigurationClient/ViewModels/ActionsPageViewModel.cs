using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using CPAPPLETLib;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ActionsPageViewModel : ObservableObject
    {
        private ObservableCollection<ClientActionWrapper> _actions = new();
        public ObservableCollection<ClientActionWrapper> Actions => _actions;

        private WMIConfigurationManagerClientService _clientService;

        public ActionsPageViewModel(WMIConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            var actions = new List<ClientActionWrapper>();
            foreach (ClientAction action in _clientService.GetClientActions())
            {
                actions.Add(new ClientActionWrapper(this, action));
            }

            foreach(var action in actions.OrderBy(a => a.ActionId))
            {
                Actions.Add(action);
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
