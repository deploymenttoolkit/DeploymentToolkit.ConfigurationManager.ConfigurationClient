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
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class GeneralPageViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CollectionViewSource))]
        private ObservableCollection<ClientProperty> _properties = new();

        public CollectionViewSource CollectionViewSource { get; } = new()
        {
            IsSourceGrouped = true
        };

        private readonly IConfigurationManagerClientService _clientService;

        private readonly Dictionary<string, Func<IWindowsManagementInstrumentationStaticInstance?>> _updateActions;

        private readonly string[] _propertiesToSkip = {
            "Namespace",
            "Class",
            "Reserved",
            "ReservedString1", "ReservedString2", "ReservedString3",
            "ReservedUint1", "ReservedUint2"
        };

        public GeneralPageViewModel(IConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            _updateActions =  new()
            {
                { "Client", _clientService.GetClient },
                { "ClientInfo", _clientService.GetClientInfo },
                { "SMSClient", _clientService.GetSMSClient },
                { "ClientIdentificationInformation", _clientService.GetClientIdentificationInformation },
                { "ClientSiteMode", _clientService.GetClientSiteMode },
                { "ClientUpgradeStatus", _clientService.GetClientUpgradeStatus },
            };

            Update();
        }

        private void Update()
        {
            Properties.Clear();

            foreach (var pair in _updateActions)
            {
                var result = pair.Value.Invoke();
                if (result == null)
                {
                    continue;
                }

                var classProperties = result.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (classProperties == null)
                {
                    continue;
                }

                foreach (var property in classProperties)
                {
                    if (_propertiesToSkip.Contains(property.Name))
                    {
                        continue;
                    }

                    Properties.Add(new ClientProperty()
                    {
                        Group = pair.Key,
                        Name = property.Name,
                        Value = property.GetValue(result)?.ToString() ?? string.Empty
                    });
                }
            }

            CollectionViewSource.Source = Properties.GroupBy(p => p.Group);
        }
    }
}
