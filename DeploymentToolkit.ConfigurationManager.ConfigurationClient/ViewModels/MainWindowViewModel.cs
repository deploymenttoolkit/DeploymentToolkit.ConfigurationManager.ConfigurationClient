using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI.Controls;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;

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
    private bool _isConnected;

    [ObservableProperty]
    private bool _isEventsConnected;

    [ObservableProperty]
    private bool _isConfigurationManagerClientInstalled;

    [ObservableProperty]
    private NavigationViewItem _selectedItem;

    [ObservableProperty]
    private string _pageTitle;

    public InAppNotification Notification { get; internal set; }

    private readonly LocalSettingsService _settings;
    private readonly NavigationService _navigationService;
    private readonly ClientConnectionManager _connectionManager;
    private readonly ThemeSelectorService _themeSelectorService;
    private readonly ClientEventsService _clientEventsService;
    private readonly IConfigurationManagerClientService _configurationManagerClientService;

    private readonly UISettings _uiSettings;

    public MainWindowViewModel(LocalSettingsService settings, NavigationService navigationService, ClientConnectionManager connectionManager, ThemeSelectorService themeSelectorService, ClientEventsService clientEventsService, IConfigurationManagerClientService configurationManagerClientService)
    {
        _settings = settings;
        _navigationService = navigationService;
        _connectionManager = connectionManager;
        _themeSelectorService = themeSelectorService;
        _clientEventsService = clientEventsService;
        _configurationManagerClientService = configurationManagerClientService;

        _connectionManager.PropertyChanged += ConnectionPropertyChanged;
        _connectionManager.Connection.PropertyChanged += ConnectionPropertyChanged;
        _clientEventsService.PropertyChanged += ConnectionPropertyChanged;

        _uiSettings = new UISettings();
        _uiSettings.ColorValuesChanged += ColorValuesChanged;

        WeakReferenceMessenger.Default.Register<NotificationMessage>(this, OnNotificationRecieved);
    }

    private void OnNotificationRecieved(object recipient, NotificationMessage message)
    {
        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            Notification.Show(message.Value, 5000);
        });
    }

    private void ColorValuesChanged(UISettings sender, object args)
    {
        App.Current.DispatcherQueue.TryEnqueue(async () =>
        {
            if (_themeSelectorService.Theme == ElementTheme.Default)
            {
                await _themeSelectorService.SetThemeAsync(ElementTheme.Default);
            }
        });
    }

    private void ConnectionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        IsConnected = _connectionManager.Connection.IsConnected;
        IsEventsConnected = _clientEventsService.IsConnected;

        if (IsConnected)
        {
            IsConfigurationManagerClientInstalled = _configurationManagerClientService.IsClientInstalled();
        }
        else
        {
            IsConfigurationManagerClientInstalled = false;
        }

        if(e.PropertyName == nameof(_connectionManager.Connection))
        {
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
    }

    [RelayCommand]
    private void OnNavigated(NavigationEventArgs navigationEventArgs)
    {
        IsBackButtonEnabled = _navigationService.CanGoBack();

        if (navigationEventArgs.Content == null || navigationEventArgs.Content is not Page page)
        {
            return;
        }

        var resourceName = $"Resources/MainWindow_Nav_{page.GetType().Name.Replace("Page", "")}/Content";
        PageTitle = resourceName.GetLocalized();
    }

    [RelayCommand]
    private void BackRequested(NavigationViewBackRequestedEventArgs navigationViewBackRequestedEventArgs)
    {
        _navigationService.GoBack();
        var currentItem = _navigationService.GetCurrentNavigationViewItem();
        if (currentItem != null)
        {
            SelectedItem = currentItem;
        }
    }

    [RelayCommand]
    private async Task ChangeThemeButtonClicked()
    {
        if(_themeSelectorService.Theme == ElementTheme.Default)
        {
            await _themeSelectorService.SetThemeAsync(App.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Light : ElementTheme.Dark);
            return;
        }
        await _themeSelectorService.SetThemeAsync(_themeSelectorService.Theme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark);
    }
}