using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using CPAPPLETLib;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ActionsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CCM_ClientAction> _actions = new();

        private IConfigurationManagerClientService _clientService;

        public ActionsPageViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            UpdateActions();
        }

        private void UpdateActions()
        {
            Actions.Clear();
            var actions = _clientService.GetClientActions();
            if(actions == null)
            {
                return;
            }

            foreach (var action in actions.OrderBy(a => a.ActionID))
            {
                action.ViewModel = this;
                Actions.Add(action);
            }
        }

        [RelayCommand]
        public void RunAction(string id)
        {
            var action = Actions.FirstOrDefault(a => a.ActionID == id);
            if(action == null)
            {
                return;
            }

            _clientService.PerformClientAction(action);
        }
    }
}
