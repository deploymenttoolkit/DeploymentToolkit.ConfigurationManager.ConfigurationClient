using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ProgramPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private ObservableCollection<CCM_Program> _programs = new();

        private readonly IConfigurationManagerClientService _clientService;

        public ProgramPageViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            Task.Factory.StartNew(() => UpdatePrograms());
        }

        [RelayCommand]
        private void UpdatePrograms()
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                Programs.Clear();
            });

            foreach(var program in _clientService.GetPrograms().OrderBy(p => p.PackageName))
            {
                program.ViewModel = this;
                App.Current.DispatcherQueue.TryEnqueue(() =>
                {
                    Programs.Add(program);
                });
            }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
                Programs.Clear();
            });
        }
    }
}
