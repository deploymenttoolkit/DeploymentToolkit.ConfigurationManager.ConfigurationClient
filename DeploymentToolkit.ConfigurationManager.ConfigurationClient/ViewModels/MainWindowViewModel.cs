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
        }


        [RelayCommand]
        private void OnSelectionChanged(NavigationViewSelectionChangedEventArgs selectionChangedEvent)
        {
            if(selectionChangedEvent.SelectedItem == null)
            {
                return;
            }
            _navigationService.Navigate(selectionChangedEvent.SelectedItem as NavigationViewItem);
            PageTitle = (selectionChangedEvent.SelectedItem as NavigationViewItem).Content.ToString();
        }

        [RelayCommand]
        private void OnNavigated(NavigationEventArgs navigationEventArgs)
        {
            IsBackButtonEnabled = _navigationService.CanGoBack();
        }
    }
}