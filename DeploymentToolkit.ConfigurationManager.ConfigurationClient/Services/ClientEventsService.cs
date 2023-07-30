using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class ClientEventsService : IDisposable
    {
        private const string _wmiQuery = "SELECT * FROM CCM_Event";
        private const string _namespace = @"root\CCM\Events";

        private const string _clientSDKwmiQuery = "SELECT * FROM CCM_InstanceEvent";
        private const string _clientSDKNamespace = @"root\CCM\ClientSDK";

        private readonly ManagementEventWatcher _eventWatcher;
        private readonly ManagementEventWatcher _clientSDKEventWatcher;
        private readonly UACService _uacService;

        public ObservableCollection<CcmEvent> Events { get; private set; } = new(); 
        public ObservableCollection<InstanceEvent> InstanceEvents { get; private set; } = new();

        public ClientEventsService(UACService uacService)
        {
            _uacService = uacService;
            if(!_uacService.IsElevated)
            {
                return;
            }

            _eventWatcher = new ManagementEventWatcher(new ManagementScope(_namespace), new EventQuery(_wmiQuery));
            _eventWatcher.EventArrived += OnEventArrived;

            _clientSDKEventWatcher = new ManagementEventWatcher(new ManagementScope(_clientSDKNamespace), new EventQuery(_clientSDKwmiQuery));
            _clientSDKEventWatcher.EventArrived += OnInstanceEventArrived;

            _eventWatcher.Start();
            _clientSDKEventWatcher.Start();
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
