using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class SoftwareUpdatesPageViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private ObservableCollection<CCM_SoftwareUpdate> _softwareUpdates = new();

        private readonly IConfigurationManagerClientService _clientService;

        public SoftwareUpdatesPageViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            WeakReferenceMessenger.Default.Register<CCMSoftwareUpdateMessage>(this, OnSoftwareUpdateMessage);

            UpdateSoftwareUpdates();
        }

        public void Dispose()
        {
            WeakReferenceMessenger.Default.Unregister<CCMSoftwareUpdateMessage>(this);
            GC.SuppressFinalize(this);
        }

        private void OnSoftwareUpdateMessage(object sender, CCMSoftwareUpdateMessage message)
        {
            if(message.Value == null)
            {
                return;
            }

            // CCM_SoftwareUpdate.UpdateID="Site_D9F97B05-F8B9-48C9-85D6-52BDF7A60F1F/SUM_9d6a91ee-cb7c-4676-94a0-f3261c5eb3ef"
            var updateId = message.Value.TargetInstancePath.Replace("CCM_SoftwareUpdate.UpdateID=", "").Replace("\"", "");
            var update = SoftwareUpdates.FirstOrDefault(u => u.UpdateID == updateId);
            if(update == null)
            {
                var softwareUpdate = new CCM_SoftwareUpdate()
                {
                    UpdateID = updateId
                };

                App.Current.DispatcherQueue.TryEnqueue(() =>
                {
                    SoftwareUpdates.Add(_clientService.UpdateInstance<CCM_SoftwareUpdate>(softwareUpdate));
                });
                return;
            }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                _clientService.UpdateInstance<CCM_SoftwareUpdate>(update);
            });
        }

        [RelayCommand]
        private void UpdateSoftwareUpdates()
        {
            SoftwareUpdates.Clear();
            foreach(var update in _clientService.GetSoftwareUpdates().OrderBy(u => u.Name))
            {
                SoftwareUpdates.Add(update);
            }
        }
    }
}
