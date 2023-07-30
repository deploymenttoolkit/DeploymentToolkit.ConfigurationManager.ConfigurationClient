using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ClientEventsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private Visibility _errorMessageVisibility = Visibility.Collapsed;

        private readonly UACService _uacService;
        private readonly ClientEventsService _clientEventsService;

        public ObservableCollection<CcmEvent> Events { get; private set; } = new();

        public ClientEventsPageViewModel(UACService uacService, ClientEventsService clientEventsService)
        {
            _uacService = uacService;
            _clientEventsService = clientEventsService;

            if(!uacService.IsElevated)
            {
                ErrorMessageVisibility = Visibility.Collapsed;
                return;
            }

            foreach(var item in _clientEventsService.Events)
            {
                Events.Add(item);
            }
            _clientEventsService.Events.CollectionChanged += Events_CollectionChanged;
        }

        private void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var item in e.NewItems)
                        {
                            Events.Add(item as CcmEvent);
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.NewItems)
                        {
                            Events.Remove(item as CcmEvent);
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Reset:
                    {
                        Events.Clear();
                        break;
                    }
                }
            });
        }

        [RelayCommand]
        private void RestartButton()
        {
            _uacService.RestartAsAdmin();
        }
    }
}
