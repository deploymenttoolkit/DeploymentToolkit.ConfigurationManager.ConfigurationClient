using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBackButtonEnabled;

        [ObservableProperty]
        private bool _isWMIConnected;

        [ObservableProperty]
        private NavigationViewItem _selectedItem;

        [ObservableProperty]
        private string _pageTitle;

        private readonly NavigationService _navigationService;
        private readonly IWindowsManagementInstrumentationConnection _wmiConnection;

        public MainWindowViewModel(NavigationService navigationService, IWindowsManagementInstrumentationConnection wmiConnection)
        {
            _navigationService = navigationService;
            _wmiConnection = wmiConnection;

            _navigationService.Navigate("Settings");
            
            _wmiConnection.PropertyChanged += WMIConnection_PropertyChanged;
        }

        private void WMIConnection_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_wmiConnection.IsConnected))
            {
                IsWMIConnected = _wmiConnection.IsConnected;
            }
        }

        [RelayCommand]
        private void OnSelectionChanged(NavigationViewSelectionChangedEventArgs selectionChangedEvent)
        {
            if(selectionChangedEvent.SelectedItem == null || selectionChangedEvent.SelectedItem is not NavigationViewItem item)
            {
                return;
            }

            _navigationService.Navigate(item);
            PageTitle = item.Content?.ToString() ?? nameof(item);
        }

        [RelayCommand]
        private void OnNavigated(NavigationEventArgs navigationEventArgs)
        {
            IsBackButtonEnabled = _navigationService.CanGoBack();
        }
    }
}