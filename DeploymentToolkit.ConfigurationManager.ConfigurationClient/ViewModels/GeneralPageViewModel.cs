using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using CommunityToolkit.Mvvm.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Management;
using Microsoft.UI.Xaml.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public class GeneralPageViewModel : ObservableObject
    {
        private ObservableGroupedCollection<string, ClientProperty> _properties;
        public ReadOnlyObservableGroupedCollection<string, ClientProperty> Properties { get; }
        public CollectionViewSource CollectionViewSource { get; }

        private readonly ConfigurationManagerClientService _clientService;

        private readonly Dictionary<string, Func<ManagementBaseObject>> _updateActions;

        public GeneralPageViewModel(ConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            _updateActions =  new()
            {
                { "Client", _clientService.GetClient },
                { "ClientInfo", _clientService.GetClientInfo },
                { "SMSClient", _clientService.GetSMSClient },
                { "SMSMP", _clientService.GetSMSLookupMP },
                { "ClientIdentificationInformation", _clientService.GetClientIdentificationInformation },
                { "ClientSiteMode", _clientService.GetClientSiteMode },
                { "ClientUpgradeStatus", _clientService.GetClientUpgradeStatus },
            };

            var properties = new Collection<ClientProperty>();

            foreach (var pair in _updateActions)
            {
                foreach (var property in pair.Value.Invoke().Properties)
                {
                    properties.Add(new ClientProperty()
                    {
                        Group = pair.Key,
                        Name = property.Name,
                        Value = property.Value?.ToString() ?? string.Empty,
                    });
                }
            }

            _properties = new(properties.GroupBy(p => p.Group));
            Properties = new(_properties);
            CollectionViewSource = new CollectionViewSource()
            {
                IsSourceGrouped = true,
                Source = Properties
            };
        }
    }
}
