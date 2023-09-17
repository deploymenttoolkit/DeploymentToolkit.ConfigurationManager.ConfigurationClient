using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
#if DEBUG
        public Visibility DebugVisibility { get; set; } = Visibility.Visible;
#else
        public Visibility DebugVisibility { get; set; } = Visibility.Collapsed;
#endif

        [ObservableProperty]
        private bool _isBackButtonEnabled;

        [ObservableProperty]
        private bool _isWMIConnected;

        [ObservableProperty]
        private NavigationViewItem _selectedItem;

        [ObservableProperty]
        private string _pageTitle;

        private readonly NavigationService _navigationService;
        private readonly ClientConnectionManager _connectionManager;
        private readonly ThemeSelectorService _themeSelectorService;

        public MainWindowViewModel(NavigationService navigationService, ClientConnectionManager connectionManager, ThemeSelectorService themeSelectorService)
        {
            _navigationService = navigationService;
            _connectionManager = connectionManager;
            _themeSelectorService = themeSelectorService;

            _navigationService.Navigate("Settings");

            _connectionManager.PropertyChanged += ConnectionPropertyChanged;
            _connectionManager.Connection.PropertyChanged += ConnectionPropertyChanged;
        }

        private void ConnectionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_connectionManager.Connection.IsConnected))
            {
                IsWMIConnected = _connectionManager.Connection.IsConnected;
            }
            else if(e.PropertyName == nameof(_connectionManager.Connection))
            {
                IsWMIConnected = _connectionManager.Connection.IsConnected;
                _connectionManager.Connection.PropertyChanged -= ConnectionPropertyChanged;
                _connectionManager.Connection.PropertyChanged += ConnectionPropertyChanged;
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

        [RelayCommand]
        private async Task ChangeThemeButtonClicked()
        {
            await _themeSelectorService.SetThemeAsync(_themeSelectorService.Theme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark);
        }
    }
}