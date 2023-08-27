using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private NavigationViewItem _selectedItem;

        [ObservableProperty]
        private string _pageTitle;

        private readonly NavigationService _navigationService;

        public MainWindowViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.Navigate("Settings");
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