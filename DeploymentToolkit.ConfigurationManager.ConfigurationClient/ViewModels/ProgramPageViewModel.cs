using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ProgramPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CCM_Program> _programs = new();

        private readonly IConfigurationManagerClientService _clientService;

        public ProgramPageViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            UpdatePrograms();
        }

        [RelayCommand]
        private void UpdatePrograms()
        {
            Programs.Clear();
            foreach(var program in _clientService.GetPrograms().OrderBy(p => p.PackageName))
            {
                program.ViewModel = this;
                Programs.Add(program);
            }
        }
    }
}
