using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ClientEventsPageViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private Visibility _errorMessageVisibility = Visibility.Collapsed;

        private const string _wmiQuery = "SELECT * FROM CCM_Event";
        private const string _namespace = @"root\CCM\Events";

        private readonly ManagementEventWatcher _eventWatcher;
        private readonly UACService _uacService;

        public ObservableCollection<CcmEvent> Events { get; private set; } = new();

        public ClientEventsPageViewModel(UACService uacService)
        {
            _uacService = uacService;
            if(!uacService.IsElevated)
            {
                ErrorMessageVisibility = Visibility.Collapsed;
                return;
            }

            _eventWatcher = new ManagementEventWatcher(new ManagementScope(_namespace), new EventQuery(_wmiQuery));
            _eventWatcher.EventArrived += OnEventArrived;
            _eventWatcher.Start();
        }

        private void OnEventArrived(object sender, EventArrivedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(e.NewEvent.GetPropertyValue("__CLASS"));
#endif

            var ccmEvent = new CcmEvent(e.NewEvent);
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                Events.Add(ccmEvent);
            });
        }

        public void Dispose()
        {
            _eventWatcher.Stop();
            _eventWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }

        [RelayCommand]
        private void RestartButton()
        {
            _uacService.RestartAsAdmin();
        }
    }
}
