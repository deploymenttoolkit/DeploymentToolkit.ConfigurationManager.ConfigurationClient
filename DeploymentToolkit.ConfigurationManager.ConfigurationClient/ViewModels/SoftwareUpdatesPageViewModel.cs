using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class SoftwareUpdatesPageViewModel : ObservableObject, IDisposable
    {
        private ObservableCollection<SoftwareUpdate> _softwareUpdates = new();
        public ObservableCollection<SoftwareUpdate> SoftwareUpdates => _softwareUpdates;

        private readonly ConfigurationManagerClientService _clientService;

        public SoftwareUpdatesPageViewModel(ConfigurationManagerClientService clientService)
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

        internal ManagementObject GetSoftwareUpdate(string id) => _clientService.GetSoftwareUpdate(id);

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
                return;
            }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                update.UpdateInstance(true);
            });
        }

        [RelayCommand]
        private void UpdateSoftwareUpdates()
        {
            var updates = new List<SoftwareUpdate>();
            foreach (ManagementObject update in _clientService.GetSoftwareUpdates())
            {
                updates.Add(new SoftwareUpdate(this, update));
            }

            SoftwareUpdates.Clear();
            foreach(var update in updates.OrderBy(u => u.Name))
            {
                SoftwareUpdates.Add(update);
            }
        }

    }
}
