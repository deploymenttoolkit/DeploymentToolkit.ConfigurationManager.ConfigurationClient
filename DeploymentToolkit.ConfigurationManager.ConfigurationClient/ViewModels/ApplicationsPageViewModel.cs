using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ApplicationsPageViewModel : ObservableObject, IDisposable
    {
        private ObservableCollection<Application> _applications = new();
        public ObservableCollection<Application> Applications => _applications;

        private WMIConfigurationManagerClientService _clientService;

        [ObservableProperty]
        private DateTime _lastUpdated;

        public ApplicationsPageViewModel(WMIConfigurationManagerClientService clientService)
        {
            _clientService = clientService;

            WeakReferenceMessenger.Default.Register<CCMApplicationMessage>(this, OnApplicationMessage);

            UpdateApplications();
        }

        public void Dispose()
        {
            WeakReferenceMessenger.Default.Unregister<CCMApplicationMessage>(this);
            GC.SuppressFinalize(this);
        }

        public ManagementBaseObject GetApplication(string id, string revision, bool isMachineTarget) => _clientService.GetApplication(id, revision, isMachineTarget);

        private void OnApplicationMessage(object recipient, CCMApplicationMessage message)
        {
            if(message.Value == null)
            {
                return;
            }

            // CCM_Application.Id="ScopeId_D9F97B05-F8B9-48C9-85D6-52BDF7A60F1F/Application_63a3eeac-4ab0-47f4-bebd-8563e256bd12",Revision=1,IsMachineTarget=1
            var applicationId = message.Value.TargetInstancePath.Substring(0, message.Value.TargetInstancePath.IndexOf(','))
                .Replace("CCM_Application.Id=", "").Replace("\"", "");
            var application = Applications.FirstOrDefault(u => u.Id == applicationId);
            if (application == null)
            {
                return;
            }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                application.UpdateInstance(true);
            });
        }

        [RelayCommand]
        private void UpdateApplications()
        {
            var applications = new List<Application>();
            foreach(ManagementObject application in _clientService.GetApplications())
            {
                applications.Add(new Application(this, application));
            }

            Applications.Clear();
            foreach(var application in applications.OrderBy(a => a.Name))
            {
                Applications.Add(application);
            }

            LastUpdated = DateTime.Now;
        }

        [RelayCommand]
        private void Install(Application application)
        {
            _clientService.InstallApplication(application, Priority.Foreground);
        }

        [RelayCommand]
        private void Repair(Application application)
        {
            _clientService.RepairApplication(application, Priority.Foreground);
        }

        [RelayCommand]
        private void Uninstall(Application application)
        {
            _clientService.UninstallApplication(application, Priority.Foreground);
        }
    }
}
