using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class ClientEventsService : ObservableObject, IDisposable
    {
        private const string _wmiQuery = "SELECT * FROM CCM_Event";

        private const string _clientSDKwmiQuery = "SELECT * FROM CCM_InstanceEvent";

        private ManagementEventWatcher? _eventWatcher;
        private ManagementEventWatcher? _clientSDKEventWatcher;

        private readonly ILogger<ClientEventsService> _logger;
        private readonly UACService _uacService;

        [ObservableProperty]
        private bool _isConnected;

        public ObservableCollection<CcmEvent> Events { get; private set; } = new(); 
        public ObservableCollection<InstanceEvent> InstanceEvents { get; private set; } = new();

        public ClientEventsService(ILogger<ClientEventsService> logger, UACService uacService)
        {
            _logger = logger;
            _uacService = uacService;
        }

        internal bool Connect(ManagementScope clientEventsNamespace, ManagementScope clientSDKNamespace)
        {
            if (!_uacService.IsElevated)
            {
                return false;
            }

            try
            {
                _eventWatcher = new ManagementEventWatcher(clientEventsNamespace, new EventQuery(_wmiQuery));
                _eventWatcher.EventArrived += OnEventArrived;

                _clientSDKEventWatcher = new ManagementEventWatcher(clientSDKNamespace, new EventQuery(_clientSDKwmiQuery));
                _clientSDKEventWatcher.EventArrived += OnInstanceEventArrived;

                _eventWatcher.Start();
                _clientSDKEventWatcher.Start();

                IsConnected = true;

                return true;
            }
            catch(ManagementException ex)
            {
                _logger.LogError(ex, "Failed to connect to events wmi");
                return false;
            }
        }

        internal bool Disconnect()
        {
            _eventWatcher?.Stop();
            _eventWatcher?.Dispose();
            _eventWatcher = null;

            _clientSDKEventWatcher?.Stop();
            _clientSDKEventWatcher?.Dispose();
            _clientSDKEventWatcher = null;

            IsConnected = false;

            return true;
        }

        private void OnInstanceEventArrived(object sender, EventArrivedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"InstanceEvent: {e.NewEvent.GetPropertyValue("__CLASS")}");
#endif
            var instanceEvent = new InstanceEvent(e.NewEvent);
            InstanceEvents.Add(instanceEvent);
        }

        private void OnEventArrived(object sender, EventArrivedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"Event: {e.NewEvent.GetPropertyValue("__CLASS")}");
#endif

            var ccmEvent = new CcmEvent(e.NewEvent);
            Events.Add(ccmEvent);
        }

        public void Dispose()
        {
            _eventWatcher.Stop();
            _eventWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
