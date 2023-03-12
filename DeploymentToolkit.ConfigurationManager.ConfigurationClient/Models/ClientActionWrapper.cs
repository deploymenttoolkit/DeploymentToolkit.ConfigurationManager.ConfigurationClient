using CommunityToolkit.Mvvm.ComponentModel;
using CPAPPLETLib;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class ClientActionWrapper : ObservableObject
    {
        public ActionsPageViewModel ViewModel { get; private set; }

        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _actionId;

        private ClientAction _action;

        public ClientActionWrapper(ActionsPageViewModel viewModel, ClientAction clientAction)
        {
            _action = clientAction;
            ViewModel = viewModel;

            DisplayName = clientAction.Name;
            ActionId = clientAction.ActionID;
        }

        public void PerformAction() => _action.PerformAction();
    }
}
