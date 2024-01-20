using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels.SoftwareCenter;

public partial class ApplicationsPageViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private ObservableCollection<CCM_Application> _applications = new();
    [ObservableProperty]
    private bool _isLoading = true;
    [ObservableProperty]
    private DateTime _lastUpdated;

    private readonly IConfigurationManagerClientService _clientService;

    public ApplicationsPageViewModel(IConfigurationManagerClientService clientService)
    {
        _clientService = clientService;

        WeakReferenceMessenger.Default.Register<CCMApplicationMessage>(this, OnApplicationMessage);

        Task.Factory.StartNew(() => UpdateApplications());
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.Unregister<CCMApplicationMessage>(this);
        GC.SuppressFinalize(this);
    }

    private void OnApplicationMessage(object recipient, CCMApplicationMessage message)
    {
        if(message.Value == null || string.IsNullOrEmpty(message.Value.TargetInstancePath))
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
            _clientService.UpdateInstance<CCM_Application>(application);
        });
    }

    [RelayCommand]
    private void UpdateApplications()
    {
        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            IsLoading = true;
            Applications.Clear();
        });

        foreach(var application in _clientService.GetApplications().OrderBy(a => a.Name))
        {
            application.ViewModel = this;
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                Applications.Add(application);
            });
        }

        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            LastUpdated = DateTime.Now;
            IsLoading = false;
        });
    }

    [RelayCommand]
    private void Install(CCM_Application application)
    {
        _clientService.InstallApplication(application, Priority.Foreground);
    }

    [RelayCommand]
    private void Repair(CCM_Application application)
    {
        _clientService.RepairApplication(application, Priority.Foreground);
    }

    [RelayCommand]
    private void Uninstall(CCM_Application application)
    {
        _clientService.UninstallApplication(application, Priority.Foreground);
    }
}
