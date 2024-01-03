using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;

public partial class ActionsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CCM_ClientAction> _actions = new();
    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _isAdministrator;

    private readonly IConfigurationManagerClientService _clientService;
    private readonly UACService _uacService;

    public ActionsPageViewModel(IConfigurationManagerClientService clientService, UACService uacService)
    {
        _clientService = clientService;
        _uacService = uacService;

        IsAdministrator = _uacService.IsElevated;
        if(!IsAdministrator)
        {
            IsLoading = false;
            return;
        }

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

    [RelayCommand]
    private void RestartButton()
    {
        _uacService.RestartAsAdmin();
    }
}
