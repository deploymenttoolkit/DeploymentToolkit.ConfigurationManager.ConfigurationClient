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
        private ObservableCollection<Models.Program> _programs = new();
        public ObservableCollection<Models.Program> Programs => _programs;

        private ConfigurationManagerClientService _clientService;

        public ProgramPageViewModel(ConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            UpdatePrograms();
        }

        [RelayCommand]
        private void UpdatePrograms()
        {
            var programs = new List<Models.Program>();
            foreach(var program in _clientService.GetPrograms())
            {
                programs.Add(new Models.Program(this, program));
            }

            Programs.Clear();
            foreach(var program in programs.OrderBy(p => p.PackageName))
            {
                Programs.Add(program);
            }
        }
    }
}
