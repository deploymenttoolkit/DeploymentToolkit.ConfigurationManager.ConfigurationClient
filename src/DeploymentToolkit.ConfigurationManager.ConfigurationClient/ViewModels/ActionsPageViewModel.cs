using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ActionsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CCM_ClientAction> _actions = new();
        [ObservableProperty]
        private bool _isLoading = true;

        private readonly IConfigurationManagerClientService _clientService;

        public ActionsPageViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            Task.Factory.StartNew(() => UpdateActions());
        }

        private void UpdateActions()
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                Actions.Clear();
                IsLoading = true;
            });
            

            var actions = _clientService.GetClientActions();
            if(actions == null)
            {
                return;
            }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                foreach (var action in actions.OrderBy(a => a.ActionID))
                {
                    action.ViewModel = this;
                    Actions.Add(action);
                }

                IsLoading = false;
            });
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
