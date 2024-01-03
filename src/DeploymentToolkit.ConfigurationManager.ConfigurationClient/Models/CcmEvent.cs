using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public enum EventSeverity : uint
    {
        Info,
        Warning,
        Error
    }

    public partial class CcmEvent : ObservableObject
    {
        [ObservableProperty]
        private string _className;

        [ObservableProperty]
        private DateTime _received;
        [ObservableProperty]
        private string _clientId;
        [ObservableProperty]
        private uint _processId;
        [ObservableProperty]
        private uint _threadId;
        [ObservableProperty]
        private EventSeverity _severity;

        public ObservableCollection<WindowsManagementInstrumentationProperty> Properties { get; set; } = new();

        private static readonly List<string> _defaultProperties = new()
        {
            "DateTime",
            "ClientID",
            "ProcessID",
            "ThreadID",
            "Severity"
        };

        public CcmEvent(ManagementBaseObject ccmEvent)
        {
            ClassName = ccmEvent.GetPropertyValue("__CLASS") as string;

            Received = ManagementDateTimeConverter.ToDateTime(ccmEvent.GetPropertyValue("DateTime") as string);
            ClientId = ccmEvent.GetPropertyValue("ClientID") as string;
            ProcessId = Convert.ToUInt32(ccmEvent.GetPropertyValue("ProcessID"));
            ThreadId = Convert.ToUInt32(ccmEvent.GetPropertyValue("ThreadID"));
            Severity = (EventSeverity)(uint)ccmEvent.GetPropertyValue("Severity");

            foreach (var property in ccmEvent.Properties)
            {
                if (_defaultProperties.Contains(property.Name))
                {
                    continue;
                }
                Properties.Add(new WindowsManagementInstrumentationProperty(property.Name, property.Value));
            }
        }
    }
}
