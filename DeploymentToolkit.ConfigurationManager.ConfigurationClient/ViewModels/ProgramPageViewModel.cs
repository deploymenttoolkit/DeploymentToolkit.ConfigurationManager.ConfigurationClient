using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ProgramPageViewModel : ObservableObject
    {
        private ObservableCollection<Models.CCM.ClientSDK.CCM_Program> _programs = new();
        public ObservableCollection<Models.CCM.ClientSDK.CCM_Program> Programs => _programs;

        private WMIConfigurationManagerClientService _clientService;

        public ProgramPageViewModel(WMIConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            UpdatePrograms();
        }

        [RelayCommand]
        private void UpdatePrograms()
        {
            var programs = new List<Models.CCM.ClientSDK.CCM_Program>();
            foreach(var program in _clientService.GetPrograms())
            {
                programs.Add(new Models.CCM.ClientSDK.CCM_Program(this, program));
            }

            Programs.Clear();
            foreach(var program in programs.OrderBy(p => p.PackageName))
            {
                Programs.Add(program);
            }
        }
    }
}
